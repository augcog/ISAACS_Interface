﻿namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using ROSBridgeLib.interface_msgs;

    public class Drone
    {
        public GameObject gameObjectPointer; // This is the related game object
        public DroneProperties droneProperties; // TODO: remove
        public int id; // This is the identifier of the drone in the dronesDict and across the ROSBridge
        public bool selected;

        public List<Waypoint> waypoints; // All waypoints held by the drone
        private List<Waypoint> deletedWaypoints; // All waypoints that were deleted, in case the player wants to redo them.

        public List<ROSSensorConnectionInterface> attachedSensors; // List of attached sensor gameobjects

        // TODO: description. Helper method for waypoint arrays.
        private bool isEmptyWaypointList(List<Waypoint> waypointList)
        {
            return waypointList != null && waypointList.Count > 0;
        }


    
        /// <summary>
        /// Constructor method for Drone class objects
        /// </summary>
        /// <param name="drone_obj"> We pass in a Gameobject for the drone -- this will be phased out and the new drone_obj gameObject will be instantiated in this method </param>
        public Drone(Vector3 position, int uniqueID)
        {
            // Create gameObject at position
            GameObject baseObject = (GameObject)WorldProperties.worldObject.GetComponent<WorldProperties>().droneBaseObject;
            gameObjectPointer = Object.Instantiate(baseObject, position, Quaternion.identity);

            Debug.Log("Position init: " + position.ToString());
            droneProperties = gameObjectPointer.GetComponent<DroneProperties>();
            droneProperties.enabled = true;
            droneProperties.droneClassPointer = this; // Connect the gameObject back to the classObject
            droneProperties.selectedMeshRenderer = gameObjectPointer.transform.Find("group3/Outline").GetComponent<MeshRenderer>();

            gameObjectPointer.tag = "Drone";
            gameObjectPointer.name = baseObject.name;
            gameObjectPointer.transform.localScale = WorldProperties.actualScale / 5;
            gameObjectPointer.transform.parent = WorldProperties.worldObject.transform;

            WorldProperties.AddClipShader(gameObjectPointer.transform);

            // Initialize path and placement order lists
            waypoints = new List<Waypoint>(); // All waypoints held by the drone
            deletedWaypoints = new List<Waypoint>(); // All waypoints that were deleted, in case the player wants to redo them.


            // Updating the world properties to reflect a new drone being added
            id = uniqueID;
            WorldProperties.AddDrone(this);

            Debug.Log("Created new drone with id: " + id);

            // Initilize the sensor list
            // @Jasmine: this is populated by ROS Manager when initilzing the drone and can be used for the UI
            // @Jasmine: feel free to add/remove functionality as needed, it's a very rough structure right now
            attachedSensors = new List<ROSSensorConnectionInterface>();

            // Init as unselected
            gameObjectPointer.transform.Find("group3/Outline").GetComponent<MeshRenderer>().material = droneProperties.deselectedMaterial;
            selected = false;
        }


        /// <summary>
        /// Use this to add a new Waypoint to the end of the drone's path
        /// </summary>
        /// <param name="coordinates">The coordinates of the waypoint which is to be added to the end of path.</param>        
        /// <returns>The waypoint which is to be added to the end of path.</returns>        
        public Waypoint AddWaypoint(Vector3 coordinates)
        {
            Debug.Log("Adding waypoint at: " + coordinates.ToString());

            string prev_id;
            // The next waypoint, that the user placed.
            Waypoint newWaypoint = new Waypoint(this, coordinates);
            Debug.Log("Created waypoint at: " + newWaypoint.ToString());

            // Check to see if we need to add the starter waypoint
            if (isEmptyWaypointList(waypoints))
            {
                // Creating the starter waypoint.
                Waypoint takeoffWaypoint = new Waypoint(this, new Vector3(0,1,0));

                Debug.Log("Creating take off waypoint at: " + takeoffWaypoint.ToString());

                // Otherwise, this is the first waypoint.
                takeoffWaypoint.nextPathPoint = newWaypoint;
                newWaypoint.prevPathPoint = takeoffWaypoint;

                // Storing this for the ROS message
                prev_id = "DRONE";
                waypoints.Add(takeoffWaypoint);

                prev_id = null;
                waypoints.Add(newWaypoint);

                Debug.Log("Added first two waypoints to the list:" + waypoints.ToString());

                newWaypoint.gameObjectPointer.GetComponent<WaypointProperties>().UpdateGroundpointLine(); // TODO: fix this jank.
                return newWaypoint; 
            }
            else
            {
                // If we don't have a line selected, we default to placing the new waypoint at the end of the path
                // Otherwise we can add as normal
                Waypoint prevWaypoint = (Waypoint)waypoints[waypoints.Count - 1]; // Grabbing the waypoint at the end of our waypoints path
                newWaypoint.prevPathPoint = prevWaypoint; // setting the previous of the new waypoint
                prevWaypoint.nextPathPoint = newWaypoint; // setting the next of the previous waypoint

                prev_id = null; // TODO: change variable name

                // Adding to dictionary, order, and path list
                waypoints.Add(newWaypoint);
                Debug.Log("Added waypoint to the list:" + waypoints.ToString());

                // Make lines mesh appear. TODO: check coloring.
                newWaypoint.gameObjectPointer.GetComponent<WaypointProperties>().UpdateGroundpointLine(); // TODO: fix this jank.
                return newWaypoint; 
            }
        }




        /// <summary>
        /// Use this to insert a new waypoint into the path (between two existing waypoints)
        /// </summary>
        /// <param name="newWaypoint"> The Waypoint which is to be added to the path </param>
        /// <param name="prevWaypoint"> The existing Waypoint just before the one which is to be added to the path </param>
        public void InsertWaypoint(Waypoint newWaypoint, Waypoint prevWaypoint)
        {
            // Adding the waypoint to the array
            int previousIndex = Mathf.Max(0, waypoints.IndexOf(prevWaypoint));
            int newIndex = previousIndex + 1;
            waypoints.Insert(newIndex, newWaypoint);

            // Inserting into the path linked list by adjusting the next and previous pointers of the surrounding waypoints
            newWaypoint.prevPathPoint = prevWaypoint;
            newWaypoint.nextPathPoint = prevWaypoint.nextPathPoint;
            
            newWaypoint.prevPathPoint.nextPathPoint = newWaypoint;
            newWaypoint.nextPathPoint.prevPathPoint = newWaypoint;
        }

        /// <summary>
        /// Use this to remove a waypoint from the path and from the scene
        /// </summary>
        /// <param name="deletedWaypoint"> The waypoint which is to be deleted </param>
        public void DeleteWaypoint(Waypoint deletedWaypoint)
        {
            // Removing the new waypoint from the dictionary, waypoints array and placement order
            waypoints.Remove(deletedWaypoint);
            deletedWaypoints.Add(deletedWaypoint);

            // Removing from the path linked list by adjusting the next and previous pointers of the surrounding waypoints. Check if first waypoint in the list.
            if (deletedWaypoint.prevPathPoint != null)
            {
                deletedWaypoint.prevPathPoint.nextPathPoint = deletedWaypoint.nextPathPoint;
            }

            // Need to check if this is the last waypoint in the list -- if it has a next or not
            if (deletedWaypoint.nextPathPoint != null)
            {
                deletedWaypoint.nextPathPoint.prevPathPoint = deletedWaypoint.prevPathPoint;
            }

            // Removing line collider
            WaypointProperties tempProperties = deletedWaypoint.gameObjectPointer.GetComponent<WaypointProperties>();
            tempProperties.DeleteLineCollider();

            // Deleting the waypoint gameObject
            Object.Destroy(deletedWaypoint.gameObjectPointer);
        }

        public Waypoint PopWaypoint()
        {
            if (!isEmptyWaypointList(waypoints))
            {
                Waypoint lastWaypoint = (Waypoint)waypoints[waypoints.Count - 1];
                DeleteWaypoint(lastWaypoint);
                return lastWaypoint;
            }
            else
            {
                return null;
            }

        }


        public void RestoreLastlyDeletedWaypoint()
        {
            if (!isEmptyWaypointList(deletedWaypoints))
            {
                Waypoint restoredWaypoint = (Waypoint)deletedWaypoints[0];
                AddWaypoint(restoredWaypoint.gameObjectPointer.transform.position);
                deletedWaypoints.Remove(restoredWaypoint);
            }
        }


        /// <summary>
        /// Removes all waypoints from this drone (including the first one).
        /// </summary>
        // TODO: Fix 
        public void DeleteAllWaypoints()
        {
            /*while (this.waypoints.Count > 1)
            {
                if (((Waypoint)this.waypoints[this.waypoints.Count - 1]).prevPathPoint != null)
                {
                    this.DeleteWaypoint((Waypoint)this.waypoints[this.waypoints.Count - 1]);
                    Object.Destroy(deletedWaypoint.gameObjectPointer);
                }
            }*/

            if (!isEmptyWaypointList(waypoints))
            {
                foreach (Waypoint waypoint in waypoints)
                {
                    deletedWaypoints.Add(waypoint);
                    WaypointProperties tempProperties = waypoint.gameObjectPointer.GetComponent<WaypointProperties>();
                    tempProperties.DeleteLineCollider();
                    Object.Destroy(waypoint.gameObjectPointer);
                }
                waypoints.Clear();
            }
        }




		/******************************/
		//  WAYPOINTS GETTER METHODS  //
		/******************************/

        // TODO: Description
        public int WaypointsCount()
        {
            if (isEmptyWaypointList(waypoints))
            {
                return 0;
            }
            return waypoints.Count;
        }

        // TODO: Description
        public int DeletedWaypointsCount()
        {
            if (isEmptyWaypointList(deletedWaypoints))
            {
                return 0;
            }
            return deletedWaypoints.Count;
        }

       
    }
}
