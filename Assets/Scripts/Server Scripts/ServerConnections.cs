namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using ROSBridgeLib;
    using ROSBridgeLib.std_msgs;
    using ROSBridgeLib.interface_msgs;
    using UnityEditor;
    using System.IO;
    using ISAACS;
    using SimpleJSON;
    using System;

    public class ServerConnections : MonoBehaviour
    {

        //Cant be static to have it show up in editor??
        [Header("Server Connection information")]
        public string serverIP = "0";
        public int serverPort = 0;

        [Header("ros server connection")]
        //Can we make this static to call it from Sensor_v2 scripts?
        public ROSBridgeWebSocketConnection rosServerConnection = null;

        /// Drone Subscribers supported by ISAACS System
        private enum DroneSubscribers { attitude, battery_state, flight_status, gimbal_angle, gps_health, gps_position, rtk_position, imu, rc, velocity, height_above_takeoff, local_position };

        /// Sensor types supported by ISAACS System
        private enum SensorType { PointCloud, Mesh, LAMP, PCFace, Image };

        // Sensor video players
        private enum VideoPlayers { None, LeftVideo, RightVideo, BackVideo, FrontVideo };

        /// Sensor subscribers supported by ISAACS System
        private enum SensorSubscribers
        {
            surface_pointcloud, mesh,
            colorized_points_0, colorized_points_1, colorized_points_2, colorized_points_3, colorized_points_4, colorized_points_5,
            colorized_points_faced_0, colorized_points_faced_1, colorized_points_faced_2, colorized_points_faced_3, colorized_points_faced_4, colorized_points_faced_5, fpv_camera_images
        };

        // The Drone Information provided by the server
        public class DroneInformation
        {

            public string drone_name;
            public int id;

            //public List<DroneSubscribers> droneSubscribers;
            //public bool simFlight;
            //public List<SensorInformation> attachedSensors;

            public DroneInformation(string _name, int _id)
            {
                drone_name = _name;
                id = _id;
            }

            public DroneInformation(JSONNode msg)
            {

                drone_name = msg["_name"].ToString();
                id = msg["_id"].AsInt;
            }
        }


        // The Sensor information provided by the server (utilized for unity)
        /*public class SensorInformation
        {
            public string sensorName;
            public SensorType sensorType;
            public List<SensorSubscribers> sensorSubscribers;
            public VideoPlayers videoPlayers;

            //TODO set info with set name/type/subscribers/SetColor
            public SensorInformation() //TODO
            {
              //TODO
            }

            //TODO: parse through raw msg
            public SensorInformation(JSONNode msg)
            {
              //TODO:
            }
        }*/

        void Start()
        {
            rosServerConnection = new ROSBridgeWebSocketConnection("ws://" + serverIP, serverPort);
            rosServerConnection.Connect();
            System.Threading.Thread.Sleep(5000);
            Debug.Log("created connection with: " + serverIP + " " + serverPort);
            GetAllDrones();
            //TODO: Get all sensors
            GetAllSensors();
        }

        void GetAllDrones()
        {

            string service_name = "/isaacs_server/all_drones_available";
            rosServerConnection.CallService(GetAllDronesCallback, service_name, "","[]");
            Debug.Log("called service");
        }

        public static void GetAllDronesCallback(JSONNode response)
        {
            Debug.Log(response);
            //get all_drones of objects
            //those objects have everything below
            //fields: id, drones, subs
            //we define subs object for all subscribers
               //fields: subid, messagetype
            JSONArray droneArray = response["drones_available"].AsArray;

            foreach (JSONNode droneInfoJSON in droneArray)
            {

                DroneInformation droneInfo = new DroneInformation(droneInfoJSON);
                InstantiateDrone(droneInfo);
            }
        }

        //TODO: void GetAllSensors() : will set service name similarly to GetAllDrones
        public void GetAllSensors() {
          //TODO
        }

        //TODO: static void GetAllSensorsCallback(JSONNode response): will set up json array, for each, return info, similarly to above
        public static void GetAllSensorsCallback(JSONNode response) {
          //TODO
        }

        ////TODO: InstantiateSensor(SensorInformation SensorInformation): similar to instatiatedrone below
        //public static void InstantiateSensor(SensorInformation sensorInformation) {
        //  //TODO
        //}


        public static void InstantiateDrone(DroneInformation droneInformation)
        {
            int drone_id = droneInformation.id;
            string drone_name = droneInformation.drone_name;

            /*
            List<string> droneSubscribers = new List<string>();

            foreach (DroneSubscribers subscriber in droneInformation.droneSubscribers)
            {
                droneSubscribers.Add(subscriber.ToString());
            }
            */

            // Create a new drone
            Drone_v2 droneInstance = new Drone_v2(WorldProperties.worldObject.transform.position, drone_id);
            Debug.Log("Drone Created: " + droneInstance.gameObjectPointer.name);

            DroneProperties droneProperties = droneInstance.droneProperties;
            GameObject droneGameObject = droneInstance.gameObjectPointer;

            droneGameObject.name = drone_name;
            WorldProperties.AddDrone(droneInstance);

            /*
            // Added optional funtionality we can implement as we go.

            // Create attached sensors
            foreach (ROSSensorConnectionInput rosSensorInput in rosDroneConnectionInput.attachedSensors)
            {
                ROSSensorConnectionInterface sensor = InstantiateSensor(rosSensorInput);
                droneInstance.AddSensor(sensor);
            }

            // Initilize drone sim manager script on the drone
            DroneSimulationManager droneSim = droneGameObject.GetComponent<DroneSimulationManager>();
            droneSim.InitDroneSim();
            droneProperties.droneSimulationManager = droneSim;

            // Get DroneMenu and instansiate.
            DroneMenu droneMenu = droneGameObject.GetComponent<DroneMenu>();
            droneMenu.InitDroneMenu(droneInformation, droneSubscribers);
            droneGameObject.GetComponent<DroneProperties>().droneMenu = droneMenu;
            */
        }



        public static void uploadMission(Drone_v2 drone, string ID, List<Waypoint> waypoints)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);
        }

        //TODO: Implement this (What does this do?)
        public static void uploadMissionCallback(JSONNode response)
        {
            //UploadMissionMsg result = new UploadMissionMsg(response);
        }


        //TODO
        public static void updateMission(Drone_v2 drone, string ID, List<Waypoint> updatedWaypoints)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);

            //all computation on server to see what has been changed and stuff
        }

        //TODO
        public static void updateMissionCallback(JSONNode response)
        {

        }
        //TODO
        public static void startMission(Drone_v2 drone, string ID)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);
        }

        //TODO
        public static void startMissionCallback(JSONNode response)
        {

        }

        //TODO
        public static void pauseMission(Drone_v2 drone, string ID)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);
        }

        //TODO
        public static void pauseMissionCallback(JSONNode response)
        {

        }

        //TODO
        public static void resumeMission(Drone_v2 drone, string ID)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);
        }

        //TODO
        public static void resumeMissionCallback(JSONNode response)
        {

        }

        //TODO
        public static void landDrone(Drone_v2 drone, string ID)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);
        }

        //TODO
        public static void landDroneCallback(JSONNode response)
        {

        }

        //TODO
        public static void flyHome(Drone_v2 drone, string ID)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);
        }

        //TODO
        public static void flyHomeCallback(JSONNode response)
        {

        }

        //TODO
        public static void stopDrone(Drone_v2 drone, string ID)
        {
            string service_name = "/isaacs_server/ TODO ";
            //Debug.LogFormat();
            //rosServerConnection.CallService( TODO , service_name, params);
        }

        //TODO
        public static void stopDroneCallback(JSONNode response)
        {

        }
    }
}
