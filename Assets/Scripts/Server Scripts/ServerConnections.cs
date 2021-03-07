using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.sensor_msgs;
using WebSocketSharp;
using UnityEditor;
using System.IO;
using ISAACS;
using SimpleJSON;
using System;
using System.Linq;

namespace ISAACS
{
    //author: Jasmine + Akhil + Peru
    public class ServerConnections : MonoBehaviour
    {

        //Cant be static to have it show up in editor??
        [Header("Server Connection information")]
        public string serverIP = "0";
        public int serverPort = 0;

        [Header("ros server connection")]
        //Can we make this static to call it from Sensor_v2 scripts?
        public static ROSBridgeWebSocketConnection rosServerConnection = null;
        //public static RosSocket rosSocket;

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
            //new
            //string uri = "ws://" + serverIP + ":" + serverPort;
            //rosSocket = new RosSocket(new WebSocketNetProtocol(uri));

            rosServerConnection = new ROSBridgeWebSocketConnection("ws://" + serverIP, serverPort);
            Debug.Log("created connection with: " + serverIP + " " + serverPort);
            rosServerConnection.Connect();
            System.Threading.Thread.Sleep(5000);
            //rosServerConnection.AddSubscriber("/isaacs_server/client_count", this);
            GetAllDrones();
            GetAllSensors();
        }

        // Update is called once per frame in Unity
        void Update()
        {
            if (rosServerConnection != null)
            {
                //Debug.Log("rendered!");
                rosServerConnection.Render();
            }

        }

        //Calls the server and returns every drone in the current state
        void GetAllDrones()
        {
            string service_name = "/isaacs_server/all_drones_available";
            rosServerConnection.CallService(GetAllDronesCallback, service_name, "hi", "[]");
            Debug.Log("called service");
        }

        public static void GetAllDronesCallback(JSONNode response)
        {
            Debug.Log("response gotten");
            //Debug.Log(response);

            AllDronesAvailableMsg result = new AllDronesAvailableMsg(response);
            //Debug.Log(result.getSuccess());
            if (result.getSuccess())
            {
                DroneMsg[] lst = result.getDronesAvailable();
                Debug.Log("DroneList" + lst);
                foreach (DroneMsg x in lst)
                {
                    ClientMsg drone = x.getDrone();
                    //Debug.Log("hits here");
                    DroneInformation droneInfo = new DroneInformation(drone.getName(), drone.getId());
                    InstantiateDrone(droneInfo, drone);
                }
            }
        }

        //TODO: void GetAllSensors() : will set service name similarly to GetAllDrones
        public void GetAllSensors()
        {
            //TODO
        }

        //TODO: static void GetAllSensorsCallback(JSONNode response): will set up json array, for each, return info, similarly to above
        public static void GetAllSensorsCallback(JSONNode response)
        {
            //TODO
        }

        ////TODO: InstantiateSensor(SensorInformation SensorInformation): similar to instatiatedrone below
        //public static void InstantiateSensor(SensorInformation sensorInformation) {
        //  //TODO
        //}

        // Creates a Drone_v2 object based on "droneInformation"
        public static void InstantiateDrone(DroneInformation droneInformation, ClientMsg msg)
        {
            int drone_id = droneInformation.id;
            string drone_name = droneInformation.drone_name;
            Debug.Log("made drone: " + drone_name);
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
            Debug.Log("hit here");

            // Get DroneMenu and instansiate. (OPTIONAL)
            TopicTypesMsg[] lst = msg.getTopics();
            List<string> droneSubscribers = new List<string>();
            //TODO: Is the topic types + their names the list of drone subscribers???
            foreach (TopicTypesMsg x in lst)
            {
                droneSubscribers.Add(x.getName());
            }

            DroneMenu droneMenu = droneGameObject.GetComponent<DroneMenu>();
            droneMenu.InitDroneMenu(droneSubscribers);
            droneGameObject.GetComponent<DroneProperties>().droneMenu = droneMenu;

            // Initilize drone sim manager script on the drone
            DroneSimulationManager droneSim = droneGameObject.GetComponent<DroneSimulationManager>();
            droneSim.InitDroneSim();
            droneProperties.droneSimulationManager = droneSim;



            /*
            // Added optional funtionality we can implement as we go.

            // Create attached sensors
            foreach (ROSSensorConnectionInput rosSensorInput in rosDroneConnectionInput.attachedSensors)
            {
                ROSSensorConnectionInterface sensor = InstantiateSensor(rosSensorInput);
                droneInstance.AddSensor(sensor);
            }

            */
        }

        public static void InstantiateDrone(DroneInformation droneInformation)
        {
            int drone_id = droneInformation.id;
            string drone_name = droneInformation.drone_name;
            Debug.Log("made drone: " + drone_name);

            // Create a new drone
            Drone_v2 droneInstance = new Drone_v2(WorldProperties.worldObject.transform.position, drone_id);
            Debug.Log("Drone Created: " + droneInstance.gameObjectPointer.name);

            DroneProperties droneProperties = droneInstance.droneProperties;
            GameObject droneGameObject = droneInstance.gameObjectPointer;

            droneGameObject.name = drone_name;
            WorldProperties.AddDrone(droneInstance);
        }


        //calls server to upload a new or changed mission
        public static void uploadMission(Drone_v2 drone, string ID, List<Waypoint> waypoints)
        {
            string service_name = "/isaacs_server/upload_mission";
            Debug.Log("called service");
            NavSatFixMsg[] waypointMsgs = new NavSatFixMsg[waypoints.Count];
            //change each waypoint to navsatros messages
            int count = 0;
            foreach (Waypoint x in waypoints)
            {
                Vector3 unityCoord = x.gameObjectPointer.transform.localPosition;
                GPSCoordinate rosCoord = WorldProperties.UnityCoordToGPSCoord(unityCoord);
                NavSatFixMsg msg = new NavSatFixMsg(rosCoord.Lat, rosCoord.Lng, rosCoord.Alt);
                waypointMsgs[count] = msg;
                count++;
            }
            string outputArgs = "";
            foreach (NavSatFixMsg y in waypointMsgs)
            {
                outputArgs += "{";
                outputArgs += y.ToString();
                outputArgs += "}";

            }
                
            //string outputArgs = string.Join(",", waypointMsgs.Select(x => x.ToString()).ToArray());
            Debug.Log(outputArgs);
            //TODO: might not work bc array.toString is diff than navsatfixmsg.toString
            rosServerConnection.CallService(uploadMissionCallback, service_name, ID, args: string.Format("[\'drone_id\' : {0}, \'waypoints\' : {1}]", ID, outputArgs));

        }

        public static void uploadMissionCallback(JSONNode response)
        {
            Debug.Log(response);
            UploadMissionMsg result = new UploadMissionMsg(response);
            int drone_id = result.getDroneId();
            bool success = result.getSuccess();
            string meta_data = result.getMetaData();
            if (success)
            {
                Drone_v2 drone = WorldProperties.GetDroneDict()[drone_id];
                int continueFromWaypointID = drone.droneProperties.CurrentWaypointTargetID() + 1;
                List<Waypoint> ways = drone.AllWaypoints();
                foreach (Waypoint way in ways)
                {
                    Vector3 waypoint_coord = way.gameObjectPointer.transform.localPosition;
                    float distance = Vector3.Distance(way.gameObjectPointer.transform.localPosition, waypoint_coord);

                    if (distance < 0.2f)
                    {
                        way.waypointProperties.WaypointUploaded();
                    }
                }
                drone.droneProperties.StartCheckingFlightProgress(continueFromWaypointID, ways.Count);

            }
        }

    //calls the server for any control calls regarding the mission
    public static void controlDrone(Drone_v2 drone, string ID, string command)
    {
        string service_name = "/isaacs_server/control_drone";
        //Debug.LogFormat();
        DroneCommandMsg msg = new DroneCommandMsg("");
        msg.setCommand(command);
        msg.setID(int.Parse(ID));
        //hopefully this works!
        Debug.Log(msg.ToString());
        string servargs = "[" + string.Format("\'drone_id\' : {0}, \'control_task\' : {1}", ID, msg.ToString()) + "]";
        rosServerConnection.CallService(controlDroneCallback, service_name, "bye", args: servargs);
    }

        public static void controlDroneCallback(JSONNode response)
        {
            Debug.Log("response gotten");
            DroneCommandMsg result = new DroneCommandMsg(response);
            int drone_id = result.getID();
            string command = result.getCommand();
            Drone_v2 drone = WorldProperties.GetDroneDict()[drone_id];
            //do the command stuff
            if (!result.getSuccess())
            {
                Debug.Log("Wrong drone callback/Failure to complete!");
            }
            else
            {
                switch (command)
                {
                    case "start_mission":
                        drone.onStartMission();
                        break;

                    case "pause_mission":
                        drone.onPauseMission();
                        break;

                    case "resume_mission":
                        drone.onResumeMission();
                        break;

                    case "land_drone":
                        drone.onLandDrone();
                        break;

                    case "fly_home":
                        drone.onFlyHome();
                        break;

                    default:
                        Debug.Log("Wrong drone callback!");
                        break;
                }
            }

        }

        public static void setSpeed(Drone_v2 drone, string ID, int speed)
        {

            string service_name = "/isaacs_server/set_speed";
            rosServerConnection.CallService(setSpeedCallback, service_name, ID, args: string.Format("[\'drone_id\': {0}, \'data\': {1}]", ID, speed.ToString()));

        }

        public static void setSpeedCallback(JSONNode response)
        {
            SetSpeedMsg msg = new SetSpeedMsg(response);
            //TODO: What do??

        }
    }
    }
//}
