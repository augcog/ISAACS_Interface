using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ISAACS;
using System;
using System.Threading;
using RosSharp.RosBridgeClient.Actionlib;

namespace RosSharp.RosBridgeClient
{
    public class ServerConnections2 : MonoBehaviour
    {

        public string uri = "";
        public static RosSocket rosSocket;
        //public GameObject thing;
        private List<DroneInformation> droneList = new List<DroneInformation>();

        public static UnityUploadMissionActionClient uploadActionClient;
        public static UnityControlDroneActionClient unityControlDroneActionClient;
        public static UnitySetSpeedActionClient unitySetSpeedActionClient;

        // Start is called before the first frame update
        void Start()
        {
            uploadActionClient = gameObject.GetComponent<UnityUploadMissionActionClient>();
            unityControlDroneActionClient = gameObject.GetComponent<UnityControlDroneActionClient>();
            rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol(uri));
            MessageTypes.IsaacsServer.AllDronesAvailableRequest request = new MessageTypes.IsaacsServer.AllDronesAvailableRequest();
            rosSocket.CallService<MessageTypes.IsaacsServer.AllDronesAvailableRequest, MessageTypes.IsaacsServer.AllDronesAvailableResponse>("/isaacs_server/all_drones_available", AllDronesServiceCallHandler, request);
        }

        // Update is called once per frame
        void Update()
        {
            //Some reasoning for the madness here: this is the only way we could get this to work, might be thread issue
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
                droneList.RemoveAt(0);
            }

        }

        public class DroneInformation
        {

            public string drone_name;
            public int id;
            public List<string> subscribers;

            //public List<DroneSubscribers> droneSubscribers;
            //public bool simFlight;
            //public List<SensorInformation> attachedSensors;

            public DroneInformation(string _name, int _id, List<string> _subscribers)
            {
                drone_name = _name;
                id = _id;
                subscribers = _subscribers;
            }
        }

            //Handler
        public void AllDronesServiceCallHandler(MessageTypes.IsaacsServer.AllDronesAvailableResponse message)
        {
            Debug.Log("AllDronesAvailableResponse Gotten");
            //Debug.Log(response);

            if (message.success)
            {
                MessageTypes.IsaacsServer.Drone[] lst = message.drones_available;
                Debug.Log("DroneList" + lst);
                foreach (MessageTypes.IsaacsServer.Drone drone1 in lst)
                {

                    MessageTypes.IsaacsServer.TopicTypes[] lste = drone1.topics;
                    List<string> droneSubscribers = new List<string>();
                    //TODO: Is the topic types + their names the list of drone subscribers???
                    foreach (MessageTypes.IsaacsServer.TopicTypes x in lste)
                    {
                        droneSubscribers.Add(x.name);
                    }

                    DroneInformation droneInfo = new DroneInformation(drone1.name, (int)drone1.id, droneSubscribers);
                    droneList.Add(droneInfo);
                }
            }
        }

        //SET SPEED
        //TODO: Change to ACTION
        public static void setSpeed(Drone_v2 drone, int ID, float speed)
        {
            Debug.Log("sent set speed service");
            //MessageTypes.IsaacsServer.SetSpeedRequest request = new MessageTypes.IsaacsServer.SetSpeedRequest((uint) ID, speed);
            //rosSocket.CallService<MessageTypes.IsaacsServer.SetSpeedRequest, MessageTypes.IsaacsServer.SetSpeedResponse>("/isaacs_server/set_speed", SetSpeedServiceCallHandler, request);

            unitySetSpeedActionClient.id = ID;
            unitySetSpeedActionClient.speed = speed;
            unitySetSpeedActionClient.RegisterGoal();
            unitySetSpeedActionClient.setSpeedActionClient.SendGoal();
        }




        //UPLOAD MISSION: Jasmine

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

            //MessageTypes.IsaacsServer.UploadMissionGoal goal = new MessageTypes.IsaacsServer.UploadMissionGoal((uint)ID, waypointsList);
            //MessageTypes.IsaacsServer.UploadMissionActionGoal action_goal = uploadActionClient.uploadMissionActionClient.action.action_goal;
            //action_goal.goal = goal;
            uploadActionClient.id = ID;
            uploadActionClient.waypoints = waypointsList;
            uploadActionClient.RegisterGoal();
            uploadActionClient.uploadMissionActionClient.SendGoal();
            //uploadActionClient.uploadMissionActionClient.SendGoal(action_goal);

            //rosSocket.CallService<MessageTypes.IsaacsServer.UploadMissionRequest, MessageTypes.IsaacsServer.UploadMissionResponse>("/isaacs_server/upload_mission", UploadMissionServiceCallHandler, request);
        }

        //public static void UploadMissionServiceCallHandler(MessageTypes.IsaacsServer.UploadMissionResponse message)
        //{
        //    Debug.Log("UploadMission Response Gotten");
        //    int drone_id = (int) message.id;
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


        //CONTROL DRONE: Akhil

        public static void controlDrone(Drone_v2 drone, int ID, string command)
        {
            Debug.Log("sent control drone service");
            //MessageTypes.IsaacsServer.ControlDroneRequest request = new MessageTypes.IsaacsServer.ControlDroneRequest((uint) ID, command);
            //rosSocket.CallService<MessageTypes.IsaacsServer.ControlDroneRequest, MessageTypes.IsaacsServer.ControlDroneResponse>("/isaacs_server/control_drone", ControlDroneServiceCallHandler, request);

            unityControlDroneActionClient.id = ID;
            unityControlDroneActionClient.command = command;
            unityControlDroneActionClient.RegisterGoal();
            unityControlDroneActionClient.controlDroneActionClient.SendGoal();
        }
    }
}
