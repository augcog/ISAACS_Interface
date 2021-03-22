using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

namespace RosSharp.RosBridgeClient
{
    public class AllDronesAvailableSubscriber : UnitySubscriber<MessageTypes.IsaacsServer.AllDronesAvailableResponse>
    {
        private MessageTypes.IsaacsServer.Drone[] drones_available;
        private bool success;
        private bool isMessageReceived;

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(MessageTypes.IsaacsServer.AllDronesAvailableResponse msg)
        {
            drones_available = msg.drones_available;
            success = msg.success;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            if (success)
            {
                MessageTypes.IsaacsServer.Drone[] lst = drones_available;
                Debug.Log("DroneList" + lst);
                foreach (MessageTypes.IsaacsServer.Drone drone in lst)
                {
                    DroneInformation droneInfo = new DroneInformation(drone.name, (int)drone.id);
                    InstantiateDrone(droneInfo, drone);
                }
            }
        }


        // The Drone Information provided by the server
        public class DroneInformation
        {

            public string drone_name;
            public int id;

            public DroneInformation(string _name, int _id)
            {
                drone_name = _name;
                id = _id;
            }

            public DroneInformation(MessageTypes.IsaacsServer.Drone msg)
            {
                drone_name = msg.name;
                id = (int)msg.id;
            }
        }

        // Creates a Drone_v2 object based on "droneInformation"
        public static void InstantiateDrone(DroneInformation droneInformation, MessageTypes.IsaacsServer.Drone msg)
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
            Debug.Log("hit here");

            // Get DroneMenu and instansiate. (OPTIONAL)
            MessageTypes.IsaacsServer.TopicTypes[] lst = msg.topics;
            List<string> droneSubscribers = new List<string>();
            //TODO: Is the topic types + their names the list of drone subscribers???
            foreach (MessageTypes.IsaacsServer.TopicTypes x in lst)
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
        }
    }
}
