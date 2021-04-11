using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ISAACS;
using RosSharp.RosBridgeClient.Actionlib;

namespace RosSharp.RosBridgeClient
{
    public class ServerConnections2 : MonoBehaviour
    {

        public string uri = "";
        public static RosSocket rosSocket;
        //public GameObject thing;

        //public static UnityControlDroneActionClient unityControlDroneActionClient;

        // Start is called before the first frame update
        void Start()
        {
            rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol(uri));
            MessageTypes.IsaacsServer.AllDronesAvailableRequest request = new MessageTypes.IsaacsServer.AllDronesAvailableRequest();
            rosSocket.CallService<MessageTypes.IsaacsServer.AllDronesAvailableRequest, MessageTypes.IsaacsServer.AllDronesAvailableResponse>("/isaacs_server/all_drones_available", AllDronesServiceCallHandler, request);
            //TODO: Testing this weird bug
            string drone_name = "drone1tester";
            int drone_id = 3;
            //Debug.Log(WorldProperties.worldObject);
            Drone_v2 droneInstance = new Drone_v2(WorldProperties.worldObject.transform.position, drone_id);
            Debug.Log("Drone Created: " + droneInstance.gameObjectPointer.name);

            DroneProperties droneProperties = droneInstance.droneProperties;
            GameObject droneGameObject = droneInstance.gameObjectPointer;

            droneGameObject.name = drone_name;
            WorldProperties.AddDrone(droneInstance);
            Debug.Log("hit here");
        }

        // Update is called once per frame
        void Update()
        {

        }

        //Handler
        private void AllDronesServiceCallHandler(MessageTypes.IsaacsServer.AllDronesAvailableResponse message)
        {
            Debug.Log("AllDronesAvailableResponse Gotten");
            //Debug.Log(response);

            if (message.success)
            {
                MessageTypes.IsaacsServer.Drone[] lst = message.drones_available;
                Debug.Log("DroneList" + lst);
                foreach (MessageTypes.IsaacsServer.Drone drone1 in lst)
                {
                    Debug.Log("made drone: " + drone1.name + " with drone id " + drone1.id);
                    string drone_name = drone1.name;
                    int drone_id = (int) drone1.id;
                    // Create a new drone
                    Debug.Log(WorldProperties.worldObject);
                    Drone_v2 droneInstance = new Drone_v2(WorldProperties.worldObject.transform.position, drone_id);
                    Debug.Log("Drone Created: " + droneInstance.gameObjectPointer.name);

                    DroneProperties droneProperties = droneInstance.droneProperties;
                    GameObject droneGameObject = droneInstance.gameObjectPointer;

                    droneGameObject.name = drone_name;
                    WorldProperties.AddDrone(droneInstance);
                    Debug.Log("hit here");

                    // Get DroneMenu and instansiate. (OPTIONAL)
                    MessageTypes.IsaacsServer.TopicTypes[] lste = drone1.topics;
                    List<string> droneSubscribers = new List<string>();
                    //TODO: Is the topic types + their names the list of drone subscribers???
                    foreach (MessageTypes.IsaacsServer.TopicTypes x in lste)
                    {
                        droneSubscribers.Add(x.name);
                    }

                    DroneMenu droneMenu = droneGameObject.GetComponent<DroneMenu>();
                    droneMenu.InitDroneMenu(droneSubscribers);
                    droneGameObject.GetComponent<DroneProperties>().droneMenu = droneMenu;

                    // Initilize drone sim manager script on the drone
                    DroneSimulationManager droneSim = droneGameObject.GetComponent<DroneSimulationManager>();
                    droneSim.InitDroneSim();
                    droneProperties.droneSimulationManager = droneSim;

                    //DroneInformation droneInfo = new DroneInformation(drone1.name, (int)drone1.id);
                    //InstantiateDrone(droneInfo, drone1);
                }
            }
        }

        //// The Drone Information provided by the server
        //public class DroneInformation
        //{

        //    public string drone_name;
        //    public int id;

        //    public DroneInformation(string _name, int _id)
        //    {
        //        drone_name = _name;
        //        id = _id;
        //    }

        //    public DroneInformation(MessageTypes.IsaacsServer.Drone msg)
        //    {
        //        drone_name = msg.name;
        //        id = (int)msg.id;
        //    }
        //}

        //// Creates a Drone_v2 object based on "droneInformation"
        //public static void InstantiateDrone(DroneInformation droneInformation, MessageTypes.IsaacsServer.Drone msg)
        //{
        //    int drone_id = droneInformation.id;
        //    string drone_name = droneInformation.drone_name;
        //    Debug.Log("made drone: " + drone_name + " with drone id " + drone_id);
        //    // Create a new drone
        //    Debug.Log(WorldProperties.worldObject);
        //    Drone_v2 droneInstance = new Drone_v2(WorldProperties.worldObject.transform.TransformPoint(Vector3.zero), drone_id);
        //    Debug.Log("Drone Created: " + droneInstance.gameObjectPointer.name);

        //    DroneProperties droneProperties = droneInstance.droneProperties;
        //    GameObject droneGameObject = droneInstance.gameObjectPointer;

        //    droneGameObject.name = drone_name;
        //    WorldProperties.AddDrone(droneInstance);
        //    Debug.Log("hit here");

        //    // Get DroneMenu and instansiate. (OPTIONAL)
        //    MessageTypes.IsaacsServer.TopicTypes[] lst = msg.topics;
        //    List<string> droneSubscribers = new List<string>();
        //    //TODO: Is the topic types + their names the list of drone subscribers???
        //    foreach (MessageTypes.IsaacsServer.TopicTypes x in lst)
        //    {
        //        droneSubscribers.Add(x.name);
        //    }

        //    DroneMenu droneMenu = droneGameObject.GetComponent<DroneMenu>();
        //    droneMenu.InitDroneMenu(droneSubscribers);
        //    droneGameObject.GetComponent<DroneProperties>().droneMenu = droneMenu;

        //    // Initilize drone sim manager script on the drone
        //    DroneSimulationManager droneSim = droneGameObject.GetComponent<DroneSimulationManager>();
        //    droneSim.InitDroneSim();
        //    droneProperties.droneSimulationManager = droneSim;
        //}

        //SET SPEED

        public static void setSpeed(Drone_v2 drone, int ID, float speed)
        {
            Debug.Log("sent set speed service");
            MessageTypes.IsaacsServer.SetSpeedRequest request = new MessageTypes.IsaacsServer.SetSpeedRequest((uint) ID, speed);
            rosSocket.CallService<MessageTypes.IsaacsServer.SetSpeedRequest, MessageTypes.IsaacsServer.SetSpeedResponse>("/isaacs_server/set_speed", SetSpeedServiceCallHandler, request);
        }

        private static void SetSpeedServiceCallHandler(MessageTypes.IsaacsServer.SetSpeedResponse message)
        {
            Debug.Log("SetSpeed Response Gotten");
            //TODO: What do??

        }


        // CONTROL DRONE
        public static void controlDrone(Drone_v2 drone, int ID, string command)
        {
            Debug.Log("sent control drone service");
            /*
             * MessageTypes.IsaacsServer.ControlDroneRequest request = new MessageTypes.IsaacsServer.ControlDroneRequest((uint) ID, command);
             * rosSocket.CallService<MessageTypes.IsaacsServer.ControlDroneRequest, MessageTypes.IsaacsServer.ControlDroneResponse>("/isaacs_server/control_drone", ControlDroneServiceCallHandler, request);
             */

            //unityControlDroneActionClient.RegisterGoal();
            //unityControlDroneActionClient.controlDroneActionClient.SendGoal();
        }
    }
}
