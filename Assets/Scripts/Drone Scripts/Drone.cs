namespace ISAACS
{
    using ROSBridgeLib.interface_msgs;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Drone 
    {
        // This is the related game object
        public GameObject gameObjectPointer;

        // The attached drone properties component
        public DroneProperties droneProperties;

        // This is the identifier of the drone in the dronesDict and across the ROSBridge
        public int id;

        // The current status of the drone
        public bool selected;

        // All waypoints held by the drone
        private List<Waypoint> waypoints;

        // All waypoints that were deleted, in case the player wants to redo them.
        private List<Waypoint> deletedWaypoints;

        // All waypoints that were deleted, in case the player wants to redo them.
        //public List<Waypoint> deletedWaypoints;

        // List of attached sensor gameobjects
        public List<ROSSensorConnectionInterface> attachedSensors;

        // The actions taken by the user for this drone, in chronological order.
        // place = placed a waypoint
        // move = moved a waypoint
        // clear = cleared all waypoints
        private Stack<string> actions;

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

            // Initialize the actions stack.
            actions = new Stack<string>();

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

            Debug.Log("Created new drone with id: " + id);
        }
        
        /// <summary>
        /// Add a sensor attached to this drone instance.
        /// </summary>
        /// <param name="sensor"></param>
        public void AddSensor(ROSSensorConnectionInterface sensor)
        {
            Debug.Log("Adding sensor: " + sensor.GetSensorName());
            attachedSensors.Add(sensor);
        }

        /******************************/
        // User waypoint interactions //
        /******************************/

        /// <summary>
        /// Use this to add a new Waypoint to the end of the drone's path
        /// </summary>
        /// <param name="coordinates">The coordinates of the waypoint which is to be added to the end of path.</param>        
        /// <returns>The waypoint which is to be added to the end of path.</returns>        
        public Waypoint AddWaypoint(Vector3 coordinates)
        {
            Debug.Log("Adding waypoint at: " + coordinates.ToString());

            // The next waypoint, that the user placed.
            Waypoint newWaypoint = new Waypoint(this, coordinates);
            Debug.Log("Created waypoint at: " + newWaypoint.ToString());

            // Check to see if we need to add the starter waypoint
            if (isEmptyWaypointList(waypoints))
            {
                // If this is the first waypoint, we need to add a starter "takeoff" waypoint before it.
                Waypoint takeoffWaypoint = new Waypoint(this, gameObjectPointer.transform.position + new Vector3(0,1,0), true);
                takeoffWaypoint.nextPathPoint = newWaypoint;
                newWaypoint.prevPathPoint = takeoffWaypoint;
                waypoints.Add(takeoffWaypoint);
                // Hide the purple plane that occurs from not having
                takeoffWaypoint.gameObjectPointer.GetComponent<LineRenderer>().enabled = false; // TODO: fix this jank.
            }
            else
            {
                // Otherwise, place a waypoint directly at the end of the waypoints list.
                Waypoint prevWaypoint = (Waypoint)waypoints[waypoints.Count - 1]; // Grabbing the waypoint at the end of our waypoints path
                newWaypoint.prevPathPoint = prevWaypoint; // setting the previous of the new waypoint
                prevWaypoint.nextPathPoint = newWaypoint; // setting the next of the previous waypoint
            }

            waypoints.Add(newWaypoint);
            Debug.Log("Added waypoint to the list:" + waypoints.ToString());
            return newWaypoint; 
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

        /// <summary>
        /// Return the last waypoint in the list if avilable
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Restore the last deleted waypoint.
        /// </summary>
        public void RestoreLastDeletedWaypoint()
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
        public void DeleteAllWaypoints()
        {
            if (!isEmptyWaypointList(waypoints))
            {
                //TODO: Test if this works (Changed 3/20/2021, Jasmine)
                Debug.Log(waypoints.Count);
                foreach (Waypoint waypoint in waypoints)
                {
                    deletedWaypoints.Add(waypoint);
                    WaypointProperties tempProperties = waypoint.gameObjectPointer.GetComponent<WaypointProperties>();
                    tempProperties.DeleteLineCollider();
                    Object.Destroy(waypoint.gameObjectPointer);
                }
                waypoints.Clear();
                waypoints.TrimExcess();
                Debug.Log(waypoints.Count);
                Debug.Log(waypoints.Capacity);
                //OLD ONE(Prior to March 2021)
                //foreach (Waypoint waypoint in waypoints)
                //{
                //    deletedWaypoints.Add(waypoint);
                //    WaypointProperties tempProperties = waypoint.gameObjectPointer.GetComponent<WaypointProperties>();
                //    tempProperties.DeleteLineCollider();
                //    Object.Destroy(waypoint.gameObjectPointer);
                //}
                //waypoints.Clear();
            }
        }

        /******************************/
        //    Waypoint Flight Logic   //
        /******************************/

        /// <summary>
        ///  To be called by ROSConnection when waypoints have been successfully uploaded.
        ///  Inform each waypoint that it has been uploaded for state change and user feedback.
        /// </summary>
        public void WaypointsUploaded(MissionWaypointMsg[] uploadedWaypoints)
        {
            foreach (MissionWaypointMsg waypoint in uploadedWaypoints)
            {

                Vector3 waypoint_coord = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(waypoint.GetLatitude(), waypoint.GetLongitude(), waypoint.GetAltitude()));

                // TODO: refine search to accound for order.
                for (int i = 1; i < waypoints.Count; i++)
                {
                    Waypoint unityWaypoint = waypoints[i];
                    float distance = Vector3.Distance(unityWaypoint.gameObjectPointer.transform.localPosition, waypoint_coord);

                    if (distance < 0.2f)
                    {
                        unityWaypoint.waypointProperties.WaypointUploaded();
                    }
                }
            }
        }

        /// <summary>
        /// To be called by any future waypoint that is edited.
        /// Relays the information to the ROSConnection.
        /// </summary>
        public void DronePathUpdated()
        {
            droneProperties.droneROSConnection.UpdateMission();
        }
        
        /******************************/
        //       HELPER METHODS       //
        /******************************/

        /// <summary>
        /// Return true if waypoint list is empty and false if not
        /// </summary>
        /// <param name="waypointList"></param>
        /// <returns></returns>
        private bool isEmptyWaypointList(List<Waypoint> waypointList)
        {
            if (waypointList == null)
            {
                return true;
            }
            return waypointList.Count == 0;
        }

        /******************************/
        //  WAYPOINTS GETTER METHODS  //
        /******************************/

        /// <summary>
        /// Get the list of all waypoints.
        /// </summary>
        /// <returns></returns>
        public List<Waypoint> AllWaypoints()
        {
            return waypoints;
        }

        /// <summary>
        /// The number of waypoints placed for the current drone.
        /// </summary>
        /// <returns>An integer, equal to the length of the waypoints list.
        public int WaypointsCount()
        {
            if (isEmptyWaypointList(waypoints))
            {
                return 0;
            }
            return waypoints.Count;
        }

        /// <summary>
        /// The number of waypoints that were placed, but got deleted for the current drone.
        /// </summary>
        /// <returns>An integer, equal to the length of the deletedWaypoints list.</returns>
        public int DeletedWaypointsCount()
        {
            if (isEmptyWaypointList(deletedWaypoints))
            {
                return 0;
            }
            return deletedWaypoints.Count; 
        }
        
        /// <summary>
        /// TReturn the waypoint at the requested index if valid
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public Waypoint GetWaypoint(int index)
        {
            if (isEmptyWaypointList(waypoints) || index >= waypoints.Count)
            {
                return null;
            }
            return (Waypoint)waypoints[index];
        }


    }
}
