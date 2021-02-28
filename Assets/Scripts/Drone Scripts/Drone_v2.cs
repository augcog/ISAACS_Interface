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
            //WorldProperties.AddDrone(this);
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

        public void setSpeed(int speed)
        {
            ServerConnections.setSpeed(this, _id, speed);
        }
        /******************************/
        //    Callback Functions      //
        /******************************/

        public void onStartMission()
        {
            //change waypoint colors?
        }

        public void onPauseMission()
        {
            //change btw pause and resume buttons?
        }

        public void onResumeMission()
        {
            //change btw pause and resume buttons?
        }

        public void onLandDrone()
        {

        }

        public void onFlyHome()
        {

        }

        //TODO: this is where unity is able to listen to changes in drone postion/etc, need to adjust it for the server itself

        /*public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
        {
            ROSBridgeMsg result = null;
            // Writing all code in here for now. May need to move out to separate handler functions when it gets too unwieldy.
            switch (topic)
            {
                case "/dji_sdk/attitude":
                    QuaternionMsg attitudeMsg = (parsed == null) ? new QuaternionMsg(raw_msg["quaternion"]) : (QuaternionMsg)parsed;
                    attitude = offset * (new Quaternion(attitudeMsg.GetX(), attitudeMsg.GetY(), attitudeMsg.GetZ(), attitudeMsg.GetW()));

                    // Update drone transform to new quaternion
                    this.transform.rotation = attitude;
                    // this.transform.localRotation = attitude;

                    if (home_attitude_set == false)
                    {
                        home_attitude = attitude;
                        home_attitude_set = true;

                        // Localize sensors when both orientation and gps position is set
                        if (home_position_set)
                        {
                            LocalizeSensors();
                        }
                    }

                    result = attitudeMsg;
                    break;
                case "/dji_sdk/battery_state":
                    battery_state = (parsed == null) ? new BatteryStateMsg(raw_msg) : (BatteryStateMsg)parsed;
                    result = battery_state;
                    break;
                case "/dji_sdk/flight_status":
                    //flight_status = (FlightStatus)(new UInt8Msg(raw_msg)).GetData();
                    break;
                case "/dji_sdk/gimbal_angle":
                    Vector3Msg gimbalAngleMsg = (parsed == null) ? new Vector3Msg(raw_msg["vector"]) : (Vector3Msg)parsed;
                    gimbal_joint_angles = new Vector3((float)gimbalAngleMsg.GetX(), (float)gimbalAngleMsg.GetY(), (float)gimbalAngleMsg.GetZ());
                    result = gimbalAngleMsg;
                    break;
                case "/dji_sdk/gps_health":
                    gps_health = (parsed == null) ? (new UInt8Msg(raw_msg)).GetData() : ((UInt8Msg)parsed).GetData();
                    break;
                case "/dji_sdk/imu":
                    imu = (parsed == null) ? new IMUMsg(raw_msg) : (IMUMsg)parsed;
                    result = imu;
                    break;
                case "/dji_sdk/rc":
                    remote_controller_msg = (parsed == null) ? new JoyMsg(raw_msg) : (JoyMsg)parsed;
                    result = remote_controller_msg;
                    break;
                case "/dji_sdk/velocity":
                    Vector3Msg velocityMsg = (parsed == null) ? new Vector3Msg(raw_msg["vector"]) : (Vector3Msg)parsed;
                    velocity = new Vector3((float)velocityMsg.GetX(), (float)velocityMsg.GetY(), (float)velocityMsg.GetZ());
                    result = velocityMsg;
                    break;
                case "/dji_sdk/height_above_takeoff":
                    relative_altitude = (parsed == null) ? (new Float32Msg(raw_msg)).GetData() : ((Float32Msg)parsed).GetData();
                    break;
                case "/dji_sdk/local_position":
                    PointMsg pointMsg = (parsed == null) ? new PointMsg(raw_msg["point"]) : (PointMsg)parsed;
                    local_position = new Vector3(pointMsg.GetX(), pointMsg.GetY(), pointMsg.GetZ());
                    result = pointMsg;
                    Debug.Log(result);
                    break;
                case "/dji_sdk/gps_position":
                case "/dji_sdk/rtk_position":
                    gps_position = (parsed == null) ? new NavSatFixMsg(raw_msg) : (NavSatFixMsg)parsed;
                    result = gps_position;
                    if (gps_position.GetLatitude() == 0.0f && gps_position.GetLongitude() == 0.0f)
                    {
                        break;
                    }

                    // TODO: Test that setting drone home latitude and longitutde as first message from drone gps position works.
                    if (home_position_set == false)
                    {
                        home_position = gps_position;
                        home_position_set = true;
                    }

                    // TODO: Complete function in World properties.
                    if (home_position_set)
                    {
                        this.transform.localPosition = WorldProperties.ROSCoordToUnityCoord(gps_position);

                        if (WorldProperties.DJI_SIM)
                        {
                            this.transform.localPosition += new Vector3(0, -100.0f, 0);
                        }
                    }

                    break;
                default:
                    Debug.LogError("Topic not implemented: " + topic);
                    break;
            }
            return result;
        }
        */
    }
}
