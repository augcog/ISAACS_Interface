namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using ROSBridgeLib.interface_msgs;

    public class Drone
    {

        public GameObject gameObjectPointer; // This is the related game object
        public char id; // This is the identifier of the drone in the dronesDict and across the ROSBridge
        public bool selected;

        public ArrayList waypoints; // All waypoints held by the drone
        public ArrayList waypointsOrder; // Keeps track of the order in which waypoints were created for the undo function

        public int nextWaypointId; // Incrementing counter to give all waypoints a unique ID when combined with the Drone ID
        public Dictionary<string, Waypoint> waypointsDict; // Collection of the waypoints in this drone's path

        public List<ROSSensorConnectionInterface> attachedSensors; // List of attached sensor gameobjects
        public ROSSensorConnectionInterface selectedSensor;

        /// <summary>
        /// Constructor method for Drone class objects
        /// </summary>
        /// <param name="drone_obj"> We pass in a Gameobject for the drone -- this will be phased out and the new drone_obj gameObject will be instantiated in this method </param>
        public Drone(Vector3 position)
        {
            // Create gameObject at position
            GameObject baseObject = (GameObject)WorldProperties.worldObject.GetComponent<WorldProperties>().droneBaseObject;
            gameObjectPointer = Object.Instantiate(baseObject, position, Quaternion.identity);
          
            Debug.Log("Position init: " + position.ToString());
            gameObjectPointer.GetComponent<DroneProperties>().classPointer = this; // Connect the gameObject back to the classObject
            gameObjectPointer.tag = "Drone";
            gameObjectPointer.name = baseObject.name;
            gameObjectPointer.transform.localScale = WorldProperties.actualScale / 5;
            gameObjectPointer.transform.parent = WorldProperties.worldObject.transform;

            WorldProperties.AddClipShader(gameObjectPointer.transform);

            // Initialize path and placement order lists
            waypoints = new ArrayList(0);
            waypointsOrder = new ArrayList(0);

            // Add waypoints container
            nextWaypointId = 0;
            waypointsDict = new Dictionary<string, Waypoint>();

            // Updating the world properties to reflect a new drone being added
            id = WorldProperties.nextDroneId;
            WorldProperties.dronesDict.Add(id, this);
            WorldProperties.nextDroneId++;

            // Initilize the sensor list
            // @Jasmine: this is populated by ROS Manager when initilzing the drone and can be used for the UI
            // @Jasmine: feel free to add/remove functionality as needed, it's a very rough structure right now
            attachedSensors = new List<ROSSensorConnectionInterface>();

            this.gameObjectPointer.transform.Find("group3").Find("Outline").GetComponent<MeshRenderer>().material = this.gameObjectPointer.GetComponent<DroneProperties>().deselectedMaterial;

            // Select this drone
            // Peru: 6/10/20: Do not select on initilization but are deselected
            this.Select();

            Debug.Log("Created new drone with id: " + id);
        }

        /// <summary>
        /// Use this to add a new Waypoint to the end of the drone's path
        /// </summary>
        /// <param name="newWaypoint"> The Waypoint which is to be added to the end of path </param>        
        public void AddWaypoint(Waypoint newWaypoint)
        {
            string prev_id;

            // Check to see if we need to add the starter waypoint
            if (waypoints.Count < 1)
            {
                //Creating the starter waypoint
                Waypoint startWaypoint = new Waypoint(this, gameObjectPointer.transform.TransformPoint(new Vector3(0,1,0)));

                // Otherwise, this is the first waypoint.
                startWaypoint.prevPathPoint = null; // This means the previous point of the path is the Drone.

                // Storing this for the ROS message
                prev_id = "DRONE";

                // Swapping the ids so the order makes sense
                string tempId = startWaypoint.id;
                startWaypoint.id = newWaypoint.id;
                Debug.Log(startWaypoint.id);
                newWaypoint.id = tempId;

                // Adding to dictionary, order, and path list
                waypointsDict.Add(startWaypoint.id, startWaypoint);
                waypoints.Add(startWaypoint);
                waypointsOrder.Add(startWaypoint);

            } else
            {
                // Otherwise we can add as normal
                Waypoint prevWaypoint = (Waypoint)waypoints[waypoints.Count - 1]; // Grabbing the waypoint at the end of our waypoints path
                newWaypoint.prevPathPoint = prevWaypoint; // setting the previous of the new waypoint
                prevWaypoint.nextPathPoint = newWaypoint; // setting the next of the previous waypoint

                // Storing this for the ROS message
                prev_id = prevWaypoint.id;

                // Adding to dictionary, order, and path list
                waypointsDict.Add(newWaypoint.id, newWaypoint);
                waypoints.Add(newWaypoint);
                waypointsOrder.Add(newWaypoint);
            }

            // Send a generic ROS ADD Update only if this is not the initial waypoint
            if (prev_id == "DRONE") {
                // we have just set the starter waypoint and still need to create the real waypoint
                this.AddWaypoint(newWaypoint);
            }
        }

        /// <summary>
        /// Use this to insert a new waypoint into the path (between two existing waypoints)
        /// </summary>
        /// <param name="newWaypoint"> The Waypoint which is to be added to the path </param>
        /// <param name="prevWaypoint"> The existing Waypoint just before the one which is to be added to the path </param>
        public void InsertWaypoint(Waypoint newWaypoint, Waypoint prevWaypoint)
        {
            // Adding the new waypoint to the dictionary and placement order
            waypointsDict.Add(newWaypoint.id, newWaypoint);
            waypointsOrder.Add(newWaypoint);

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
            waypointsDict.Remove(deletedWaypoint.id);
            waypoints.Remove(deletedWaypoint);
            waypointsOrder.Remove(deletedWaypoint);

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
        /// Use this to change which drone is selected in the world.
        /// This also changes all drone aura materials so this drone is the only yellow one.
        /// </summary>
        public void Select() {
            // Changes the color of the drone to indicate that it has been selected
            this.gameObjectPointer.transform.Find("group3/Outline").GetComponent<MeshRenderer>().material =
                this.gameObjectPointer.GetComponent<DroneProperties>().selectedMaterial;
            this.selected = true;

            WorldProperties.selectedDrone = this;

            // Check through all other drones and change their materials to deselected
            foreach (Drone otherDrone in WorldProperties.dronesDict.Values)
            {
                if (otherDrone != this)
                {
                    otherDrone.gameObjectPointer.transform.Find("group3").Find("Outline").GetComponent<MeshRenderer>().material = 
                        this.gameObjectPointer.GetComponent<DroneProperties>().deselectedMaterial;
                    otherDrone.selected = false;
                }
            }


            // TODO: Peru, Jasmine: Update SensorUI in this function.
            // Find the Sensor UI Gameobject: can be stored as a variable in world properties.
            // attachedSensors is the list of ROSSensorInterface in this class that can be send acorss.

            // init the sensor with the following:
            // Create a list of sensor UI's based on attachedSensors.
            // For each sensor UI: have the number of buttons/obtions be no. of subscribers & map every button to a subscriber id.
            // On click: call sensor function to switch ros subscriber on/off
          
            //Instantiating for sensor UI
            GameObject activeSensorName;
            Text senseTextComp;

            activeSensorName = GameObject.FindGameObjectWithTag("SENSORUI");
            senseTextComp = activeSensorName.GetComponent<Text>();

            // We call a function on sensorUIManager -> Update UI (List<ROSSensorInterface>droneSensors)
            // Then the sensors UI Manager can just display the first one
            // Left and right can just be to traverse this list


          //first sensor is always set first selectedSensor = attachedSensors.
        //check list of sensor SUBSCRIBERS, have true/false for each one based on ID?
           //make new script for the TOGGLES (cause they aren't unity buttons, should be toggles)
        }
    }
}
