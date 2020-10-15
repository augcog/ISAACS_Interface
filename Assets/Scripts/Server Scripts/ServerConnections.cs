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

        [Header("Server Connection information")]
        private static string serverIP;
        private static int serverPort;

        [Header("ros server connection")]
        private static ROSBridgeWebSocketConnection rosServerConnection = null;

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
        private class DroneInformation
        {
            public string droneName;
            public int id;
            //public List<DroneSubscribers> droneSubscribers;
            //public bool simFlight;
            //public List<SensorInformation> attachedSensors;

            public DroneInformation(JSONNode msg)
            {
                droneName = msg["droneName"].ToString();
                id = msg["droneID"].AsInt;
            }
        }

        // The Sensor information provided by the server
        private class SensorInformation
        {
            public string sensorName;
            public SensorType sensorType;
            public List<SensorSubscribers> sensorSubscribers;
            public VideoPlayers videoPlayers;
        }

        void Start()
        {
            rosServerConnection = new ROSBridgeWebSocketConnection("ws://" + serverIP, serverPort);
            GetAllDrones();
        }

        void GetAllDrones()
        {
            string service_name = "/isaacs_server/getalldrones";
            //Debug.LogFormat();
            //rosServerConnection.CallService(GetAllDronesCallback, service_name, params);
        }

        public static void GetAllDronesCallback(JSONNode response)
        {
            JSONArray droneArray = response["droneArray"].AsArray;

            foreach (JSONNode droneInfoJSON in droneArray)
            {
                DroneInformation droneInfo = new DroneInformation(droneInfoJSON);
                InstantiateDrone(droneInfo);
            }
        }

        private static void InstantiateDrone(DroneInformation droneInformation)
        {
            int drone_id = droneInformation.id;
            string drone_name = droneInformation.droneName;
            
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
    }
}