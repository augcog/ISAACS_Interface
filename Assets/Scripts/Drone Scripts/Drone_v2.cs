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

        public Drone_v2(Vector3 position, int uniqueID) : base(position, uniqueID) { }

        public void uploadMission()
        {
            //ServerConnection.uploadMission(this, _id, this.waypoints);
        }

        public void startMission()
        {
            //ServerConnection.startMission(this, _id);
        }

        public void pauseMission()
        {
            //ServerConnection.pauseMission(this, _id);
        }

        public void resumeMission()
        {
           // ServerConnection.resumeMission(this, _id);
        }

        public void landDrone()
        {
            //ServerConnection.landDrone(this, _id);
        }

        public void flyHome()
        {
            //ServerConnection.flyHome(this, _id);
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
