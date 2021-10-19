using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using RosSharp.RosBridgeClient.Actionlib;

namespace RosSharp.RosBridgeClient
{
    ///<author> Jasmine, Akhil</author>
    
    /// <summary>
    /// Class for any interactions with the server, component of World GameObject
    /// Unity Inspector: requires URI (IP address/port) and action clients as components
    /// </summary>
    public class ServerConnections2 : MonoBehaviour
    {
        //Unity Inspector variables
        public string uri = "";
        public static RosSocket rosSocket;
        public static UnityUploadMissionActionClient uploadActionClient;
        public static UnityControlDroneActionClient controlDroneActionClient;
        public static UnitySetSpeedActionClient setSpeedActionClient;


        /// <summary>
        /// List that is updated every time a drone is added, should remain empty after update
        ///     function sets up the drone, its menu, its subscribers, etc.
        /// </summary>
        private List<DroneInformation> droneList = new List<DroneInformation>();

        /// <summary>
        /// Class to simplify drone state variables for the droneList
        /// </summary>
        public class DroneInformation
        {

            public string drone_name;
            public int id;
            public List<string> subscribers;


            public DroneInformation(string _name, int _id, List<string> _subscribers)
            {
                drone_name = _name;
                id = _id;
                subscribers = _subscribers;
            }
        }

        /// <summary>
        /// Retrieves action clients attached to the gameObject, starts rosWebSocket connection with server
        /// Calls: All Drones Available service
        /// </summary>
        void Start()
        {
            uploadActionClient = gameObject.GetComponent<UnityUploadMissionActionClient>();
            controlDroneActionClient = gameObject.GetComponent<UnityControlDroneActionClient>();
            rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol(uri));
            MessageTypes.IsaacsServer.AllDronesAvailableRequest request = new MessageTypes.IsaacsServer.AllDronesAvailableRequest();
            rosSocket.CallService<MessageTypes.IsaacsServer.AllDronesAvailableRequest, MessageTypes.IsaacsServer.AllDronesAvailableResponse>("/isaacs_server/all_drones_available", AllDronesServiceCallHandler, request);
        }

        /// <summary>
        /// Once droneList is updated (once a drone is recieved), creates Drone_v2 with its properties, name, id, and subscribers
        ///     as well as its UI menu
        /// Subscribes to: Global Position  (Mavros only) - that keeps track of the position of the drone
        /// </summary>
        void Update()
        {
            //Some reasoning for the madness here: this is the only way we could get this to work due to a thread issue
            while (droneList.Count > 0)
            {
                string drone_name = droneList[0].drone_name;
                int drone_id = droneList[0].id;
                List<string> droneSubscribers = droneList[0].subscribers;
                //Debug.Log(WorldProperties.worldObject);
                Drone_v2 droneInstance = new Drone_v2(WorldProperties.worldObject.transform.position, drone_id);
                Debug.Log("Drone Created: " + droneInstance.gameObjectPointer.name);



                DroneProperties droneProperties = droneInstance.droneProperties;
                GameObject droneGameObject = droneInstance.gameObjectPointer;

                droneGameObject.name = drone_name;
                WorldProperties.AddDrone(droneInstance);
                Debug.Log("hit here");


                DroneMenu droneMenu = droneGameObject.GetComponent<DroneMenu>();
                droneMenu.InitDroneMenu(droneSubscribers);
                droneGameObject.GetComponent<DroneProperties>().droneMenu = droneMenu;

                // Initilize drone sim manager script on the drone
                DroneSimulationManager droneSim = droneGameObject.GetComponent<DroneSimulationManager>();
                droneSim.InitDroneSim();
                droneProperties.droneSimulationManager = droneSim;

                //string subscription_id = rosSocket.Subscribe<MessageTypes.Sensor.NavSatFix>("/drone_" + drone_id + "/mavros/global_position/global", droneInstance.subscriptionHandler);

                //after adding the drone, remove it from the list
                droneList.RemoveAt(0);
            }

        }



        /// <summary>
        /// Handler once an alldronesavailable response is recieved
        /// </summary>
        public void AllDronesServiceCallHandler(MessageTypes.IsaacsServer.AllDronesAvailableResponse message)
        {
            Debug.Log("AllDronesAvailableResponse Gotten");

            if (message.success)
            {
                MessageTypes.IsaacsServer.Drone[] lst = message.drones_available;
                Debug.Log("DroneList" + lst);
                foreach (MessageTypes.IsaacsServer.Drone drone1 in lst)
                {

                    MessageTypes.IsaacsServer.TopicTypes[] lste = drone1.topics;
                    List<string> droneSubscribers = new List<string>();
                    //TODO: Check if the topic types + their names are the list of drone subscribers
                    foreach (MessageTypes.IsaacsServer.TopicTypes x in lste)
                    {
                        droneSubscribers.Add(x.name);
                    }
                    DroneInformation droneInfo = new DroneInformation(drone1.name, (int)drone1.id, droneSubscribers);
                    droneList.Add(droneInfo);
                }
            }
        }

        /// <summary>
        /// Prepares the action calls to change drone speed
        /// Calls: Registers set speed action goal, sends goal
        /// </summary>
        public static void setSpeed(Drone_v2 drone, int ID, float speed)
        {
            Debug.Log("sent set speed action");
            setSpeedActionClient.id = ID;
            setSpeedActionClient.speed = speed;
            setSpeedActionClient.RegisterGoal();
            setSpeedActionClient.setSpeedActionClient.SendGoal();
        }

        /// <summary>
        /// Prepares the action calls to update and upload a waypoint mission
        /// Calls: Registers list of natsavfix messages (waypoint gps coords) and sends goal
        /// </summary>
        public static void uploadMission(Drone_v2 drone, int ID, List<Waypoint> waypoints)
        {
            Debug.Log("sent current mission for drone " + ID);
            MessageTypes.Sensor.NavSatFix[] waypointsList = new MessageTypes.Sensor.NavSatFix[waypoints.Count];
            int count = 0;
            foreach (Waypoint way in waypoints)
            {
                MessageTypes.Sensor.NavSatFix waypointItem = new MessageTypes.Sensor.NavSatFix();
                Vector3 unityCoord = way.gameObjectPointer.transform.localPosition;
                GPSCoordinate rosCoord = WorldProperties.UnityCoordToGPSCoord(unityCoord);
                waypointItem.latitude = rosCoord.Lat;
                waypointItem.longitude = rosCoord.Lng;
                waypointItem.altitude = rosCoord.Alt;
                waypointsList[count] = waypointItem;
                count++;
            }

            uploadActionClient.id = ID;
            uploadActionClient.waypoints = waypointsList;
            uploadActionClient.RegisterGoal();
            uploadActionClient.uploadMissionActionClient.SendGoal();
        }

        //TODO: Update this so it handles response from the ACTION, not the previous service call

        //public static void UploadMissionServiceCallHandler(MessageTypes.IsaacsServer.UploadMissionResponse message)
        //{
        //    Debug.Log("UploadMission Response Gotten");
        //    int drone_id = (int)message.id;
        //    bool success = message.success;
        //    string meta_data = message.message;
        //    if (success)
        //    {
        //        Drone_v2 drone = WorldProperties.GetDroneDict()[drone_id];
        //        int continueFromWaypointID = drone.droneProperties.CurrentWaypointTargetID() + 1;
        //        List<Waypoint> ways = drone.AllWaypoints();
        //        foreach (Waypoint way in ways)
        //        {
        //            Vector3 waypoint_coord = way.gameObjectPointer.transform.localPosition;
        //            float distance = Vector3.Distance(way.gameObjectPointer.transform.localPosition, waypoint_coord);

        //            if (distance < 0.2f)
        //            {
        //                way.waypointProperties.WaypointUploaded();
        //            }
        //        }
        //        drone.droneProperties.StartCheckingFlightProgress(continueFromWaypointID, ways.Count);

        //    }
        //}


        /// <summary>
        /// Prepares the action calls to start/stop/pause/resume mission + fly home + land drone
        /// Calls: Registers control_drone action goal, sends goal
        /// </summary>
        public static void controlDrone(Drone_v2 drone, int ID, string command)
        {
            Debug.Log("sent control drone service");
            controlDroneActionClient.id = ID;
            controlDroneActionClient.command = command;
            controlDroneActionClient.RegisterGoal();
            controlDroneActionClient.controlDroneActionClient.SendGoal();
        }
    }
}
