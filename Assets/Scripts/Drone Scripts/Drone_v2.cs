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
            Debug.Log("instantiates new drone_V2");
            WorldProperties.AddDrone(this);
            _id = uniqueID.ToString();
        }

        public void uploadMission()
        {
            ServerConnections.uploadMission(this, _id, this.AllWaypoints());
        }

        public void DronePathUpdated()
        {
            ServerConnections.updateMission(this, _id, this.AllWaypoints());
        }

        public void startMission()
        {
            ServerConnections.startMission(this, _id);
        }

        public void pauseMission()
        {
            ServerConnections.pauseMission(this, _id);
        }

        public void resumeMission()
        {
            ServerConnections.resumeMission(this, _id);
        }

        public void landDrone()
        {
            ServerConnections.landDrone(this, _id);
        }

        public void flyHome()
        {
            ServerConnections.flyHome(this, _id);
        }

        public void stopMission()
        {
            ServerConnections.stopDrone(this, _id);
        }

        /******************************/
        //    Callback Functions      //
        /******************************/

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
