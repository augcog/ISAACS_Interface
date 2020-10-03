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

    public class ServerConnections : MonoBehaviour
    {

        /// <summary>
        /// Drone Types supported by ISAACS System
        /// </summary>
        // Note: Currently not public because we don't want a front-facing dropdown
        enum DroneType { M100, M210, M600, Sprite };

        //TODO: Store/Pull drone list from WorldProperties instead
        private Dictionary<int, ROSDroneConnectionInterface> ROSDroneConnections = new Dictionary<int, ROSDroneConnectionInterface>();

        /// <summary>
        /// TODO: create a drone object by adding to dict ROSDroneConnections 
        /// </summary>
        void AddDrone(string ID, string IP, string port, DroneType droneType)
        {

            //TODO: What exactly are we sending to server (and how)?
        }

        //TODO: Implement this (What does this do?)
        public static void addDroneCallback()
        {

        }

        //TODO
        //what still be passed into waypoints? strings or waypoint objects or something else?
        public static void uploadMission(Drone drone, string ID, string[] waypoints)
        {

        }

        //TODO: Implement this (What does this do?)
        public static void uploadMissionCallback()
        {

        }

        //TODO
        public static void startMission(Drone drone, string ID)
        {

        }

        //TODO
        public static void startMissionCallback()
        {

        }

        //TODO
        public static void pauseMission(Drone drone, string ID)
        {

        }

        //TODO
        public static void pauseMissionCallback()
        {

        }

        //TODO
        public static void resumeMission(Drone drone, string ID)
        {

        }

        //TODO
        public static void resumeMissionCallback()
        {

        }

        //TODO
        public static void landDrone(Drone drone, string ID)
        {

        }

        //TODO
        public static void landDroneCallback()
        {

        }

        //TODO
        public static void flyHome(Drone drone, string ID)
        {

        }

        //TODO
        public static void flyHomeCallback()
        {

        }
    }
}