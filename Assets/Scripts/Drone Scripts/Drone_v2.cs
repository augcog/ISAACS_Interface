namespace ISAACS
{
    using ROSBridgeLib.interface_msgs;
    using SimpleJSON;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Drone_v2 : Drone
    {
        /// <summary>
        /// Class for drone interactions with the server
        /// Server calls are stateless so this class will hold drone state information
        /// </summary>

        private string _id;

        public Drone_v2(Vector3 position, int uniqueID) : base(position, uniqueID) {
            Debug.Log("Drone V2 Constructor: " + uniqueID.ToString());
            _id = uniqueID.ToString();
            WorldProperties.AddDrone(this);
        }

        public void uploadMission()
        {
            ServerConnections.uploadMission(this, _id, this.AllWaypoints());
        }

        public void DronePathUpdated()
        {
            ServerConnections.uploadMission(this, _id, this.AllWaypoints());
        }

        public void startMission()
        {
            ServerConnections.controlDrone(this, _id, "start_mission");
        }

        public void pauseMission()
        {
            ServerConnections.controlDrone(this, _id, "pause_mission");
        }

        public void resumeMission()
        {
            ServerConnections.controlDrone(this, _id, "resume_mission");
        }

        public void landDrone()
        {
            ServerConnections.controlDrone(this, _id, "land_drone");
        }

        public void flyHome()
        {
            ServerConnections.controlDrone(this, _id, "fly_home");
        }

        public void stopMission()
        {
            ServerConnections.controlDrone(this, _id, "stop_mission");
        }

        /******************************/
        //    Callback Functions      //
        /******************************/

        //we do these in serverconnections.cs

        public void onStartMission()
        {

        }

        public void onPauseMission()
        {

        }

        public void onResumeMission()
        {

        }

        public void onLandDronoe()
        {

        }

        public void onFlyHome()
        {

        }
    }
}
