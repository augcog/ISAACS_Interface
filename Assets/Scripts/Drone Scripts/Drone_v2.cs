namespace ISAACS
{
    using UnityEngine;
    using RosSharp.RosBridgeClient;


    ///<author> Jasmine, Akhil</author>

    /// <summary>
    /// Class for drone interactions with the server
    /// Server calls are stateless so this class will hold drone state information
    /// Logic with drone state is handled server side, not client side
    /// </summary>

    public class Drone_v2 : Drone
    {
        /// <summary>
        /// Unique identifier for the drone
        /// </summary>
        public int _id;


        /// <summary>
        /// Drone_V2 constructor, inherits from Drone.cs
        /// </summary>
        public Drone_v2(Vector3 position, int uniqueID) : base(position, uniqueID) {
            Debug.Log("Drone V2 Constructor: " + uniqueID.ToString());
            _id = uniqueID;
        }



        /// <summary>
        /// Called: when uploadMission button is pressed
        /// Calls: uploadMission() in ServerConnections2
        /// </summary>
        public void uploadMission()
        {
            ServerConnections2.uploadMission(this, id, this.AllWaypoints());
        }


        /// <summary>
        /// Called: when uploadMission button is pressed
        /// Calls: uploadMission() in ServerConnections2
        /// </summary>
        public void DronePathUpdated()
        {
           
            ServerConnections2.uploadMission(this, id, this.AllWaypoints());
        }

        /// <summary>
        /// Called: when startMission button is pressed
        /// Calls: controlDrone() in ServerConnections2 with command "start_mission"
        /// </summary>
        public void startMission()
        { 
            ServerConnections2.controlDrone(this, id, "start_mission");
        }

        /// <summary>
        /// Called: when pauseMission button is pressed
        /// Calls: controlDrone() in ServerConnections2 with command "pause_mission"
        /// </summary>
        public void pauseMission()
        {
            ServerConnections2.controlDrone(this, id, "pause_mission");
        }

        /// <summary>
        /// Called: when resumeMission button is pressed
        /// Calls: controlDrone() in ServerConnections2 with command "resume_mission"
        /// </summary>
        public void resumeMission()
        {
            //TODO: For the UI, make pause and resume mission the same button
            ServerConnections2.controlDrone(this, id, "resume_mission");
        }

        /// <summary>
        /// Called: when landDrone button is pressed
        /// Calls: controlDrone() in ServerConnections2 with command "land_drone"
        /// </summary>
        public void landDrone()
        {
            ServerConnections2.controlDrone(this, id, "land_drone");
        }

        /// <summary>
        /// Called: when flyHome button is pressed
        /// Calls: controlDrone() in ServerConnections2 with command "fly_home"
        /// </summary>
        public void flyHome()
        {
            ServerConnections2.controlDrone(this, id, "fly_home");
        }

        /// <summary>
        /// Called: when stopMission button is pressed
        /// Calls: controlDrone() in ServerConnections2 with command "stop_mission"
        /// </summary>
        public void stopMission()
        {
            ServerConnections2.controlDrone(this, id, "stop_mission");
        }


        /// <summary>
        /// Called: when startMission button is pressed
        /// Calls: setSpeed() in ServerConnections2 that sets the speed of the drone to
        ///     a float from the setSpeed Slider in the UI
        /// </summary>
        public void setSpeed(float speed)
        {
            //TODO: Test set speed slider (UI side)
            Debug.Log("passing it through");
            ServerConnections2.setSpeed(this, id, speed);
        }

        //TODO: Currently useless, can be useful, perhaps
        /******************************/
        //    Callback Functions      //
        /******************************/

        public void onStartMission()
        {
            //TODO: change waypoint colors?
        }

        public void onPauseMission()
        {
            //TODO: change btw pause and resume buttons?
        }

        public void onResumeMission()
        {
            //TODO: change btw pause and resume buttons?
        }

        public void onLandDrone()
        {

        }

        public void onFlyHome()
        {

        }


    }
}
