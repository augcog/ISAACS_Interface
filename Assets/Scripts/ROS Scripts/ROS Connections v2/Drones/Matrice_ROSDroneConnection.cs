using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;

using ISAACS;

public class Matrice_ROSDroneConnection : MonoBehaviour, ROSTopicSubscriber, ROSDroneConnectionInterface
{
    /// <summary>
    /// Drone state variables and helper enums
    /// </summary>    

    /// <summary>
    /// Current flight status of the drone
    /// </summary>
    public enum FlightStatus
    {
        ON_GROUND_STANDBY = 1,
        TAKEOFF = 2,
        IN_AIR_STANDBY = 3,
        LANDING = 4,
        FINISHING_LANDING = 5
    }

    /// <summary>
    /// Possible camera actions that can be executed
    /// </summary>
    public enum CameraAction
    {
        SHOOT_PHOTO = 0,
        START_VIDEO = 1,
        STOP_VIDEO = 2
    }

    /// <summary>
    /// Possible drone tasks that can be executed
    /// </summary>
    public enum DroneTask
    {
        GO_HOME = 1,
        TAKEOFF = 4,
        LAND = 6
    }

    /// <summary>
    /// Possible action commands during a waypoint mission
    /// </summary>
    public enum WaypointMissionAction
    {
        START = 0,
        STOP = 1,
        PAUSE = 2,
        RESUME = 3
    }

    /// <summary>
    /// ROS connection variable to the drone
    /// </summary>
    private ROSBridgeWebSocketConnection ros = null;
    /// <summary>
    /// Unique drone identifier
    /// </summary>
    string client_id;
    /// <summary>
    /// Status of drone simulator, initilized by user in editor
    /// </summary>
    bool simDrone = false;

    /// <summary>
    /// DJI SDK status
    /// </summary>
    public bool sdk_ready
    {
        get
        {
            return ros != null;
        }
    }

    /// <summary>
    /// Status of the authority of the Unity interface over the drone
    /// </summary>
    bool has_authority = false;

    /// <summary>
    /// Battery state of the drone
    /// </summary>
    BatteryStateMsg battery_state;
    
    /// <summary>
    /// Current flight status of the drone
    /// </summary>
    FlightStatus flight_status;

    /// <summary>
    /// Remote Controller commands flying the drone
    /// </summary>
    JoyMsg remote_controller_msg;
    
    /// <summary>
    /// Current attitude of the drone
    /// </summary>
    Quaternion attitude = Quaternion.identity;

    /// <summary>
    /// Current offset of the drone
    /// </summary>
    Quaternion offset = Quaternion.Euler(90, 180, 0);
    
    /// <summary>
    /// Current imu reading
    /// </summary>
    IMUMsg imu;
    
    /// <summary>
    /// Current velocity of the drone
    /// </summary>
    Vector3 velocity;
    
    /// <summary>
    /// Height of drone relative to takeoff height
    /// </summary>
    float relative_altitude;        
    
    /// <summary>
    /// Position of drone relative to set local position (not valid if no local position is set)
    /// </summary>
    Vector3 local_position;
    
    /// <summary>
    /// Current angles of gumble
    /// </summary>
    Vector3 gimble_joint_angles;
    
    /// <summary>
    /// Current gps health
    /// </summary>
    uint gps_health;
    
    /// <summary>
    /// Current gps position
    /// </summary>
    NavSatFixMsg gps_position;
    
    /// <summary>
    /// The latitude of the starting point of the drone flight
    /// </summary>
    double droneHomeLat = 0;
    
    /// <summary>
    /// The longitude of the starting point of the drone flight
    /// </summary>
    double droneHomeLong = 0;

    /// <summary>
    /// Initilize drone home position if it hasn't been set yet
    /// </summary>
    bool droneHomeSet = false;

    /// <summary>
    /// Function called by ROSManager when Drone Gameobject is initilized to start the ROS connection with requested subscribers.
    /// </summary>
    /// <param name="uniqueID"></param>
    /// <param name="droneIP"></param>
    /// <param name="dronePort"></param>
    /// <param name="droneSubscribers"></param>
    /// <param name="simFlight"></param>
    public void InitilizeDrone(int uniqueID, string droneIP, int dronePort, List<string> droneSubscribers, bool simFlight)
    {
        ros = new ROSBridgeWebSocketConnection("ws://" + droneIP, dronePort);
        client_id = uniqueID.ToString();
        simDrone = simFlight;

        foreach (string subscriber in droneSubscribers)
        {
            ros.AddSubscriber("/dji_sdk/" + subscriber, this);
        }

        // TODO: Initilize Informative UI Prefab and attach as child.
        ros.Connect();
    }

    // Update is called once per frame in Unity
    void Update()
    {
        if (ros != null)
        {
            ros.Render();
        }
    }

    /// <summary>
    /// The Control UI should call this function only
    /// Start the waypoint mission.
    /// </summary>
    public void StartMission()
    {
        // Integrate dynamic waypoint system

        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().FlyNextWaypoint();
            return;
        }

        List<MissionWaypointMsg> missionMissionMsgList = new List<MissionWaypointMsg>();

        uint[] command_list = new uint[16];
        uint[] command_params = new uint[16];

        for (int i = 0; i < 16; i++)
        {
            command_list[i] = 0;
            command_params[i] = 0;
        }

        bool skip = true;

        ArrayList waypoints = this.GetComponent<DroneProperties>().classPointer.waypoints;

        foreach (Waypoint waypoint in waypoints)
        {
            if (skip)
            {
                skip = false;
                continue;
            }

            float x = waypoint.gameObjectPointer.transform.localPosition.x;
            float y = waypoint.gameObjectPointer.transform.localPosition.y;
            float z = waypoint.gameObjectPointer.transform.localPosition.z;

            double ROS_x = WorldProperties.UnityXToLat(this.droneHomeLat, x);
            float ROS_y = (y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
            double ROS_z = WorldProperties.UnityZToLong(this.droneHomeLong, this.droneHomeLat, z);

            MissionWaypointMsg new_waypoint = new MissionWaypointMsg(ROS_x, ROS_z, ROS_y, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
            Debug.Log("single waypoint info: " + new_waypoint);
            missionMissionMsgList.Add(new_waypoint);
        }
        MissionWaypointTaskMsg Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.AUTO_LANDING, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.POINT, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, missionMissionMsgList.ToArray());
        UploadWaypointsTask(Task);

        // In UploadWaypointsTask Response start the mission
        //SendWaypointAction(WaypointMissionAction.START);
    }

    /// <summary>
    /// The Control UI should call this function only
    /// Pause an active mission.
    /// </summary>
    public void PauseMission()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().pauseFlight();
            return;
        }

        SendWaypointAction(WaypointMissionAction.PAUSE);
    }

    /// <summary>
    /// The Control UI should call this function only
    /// Resume a paused mission.
    /// </summary>
    public void ResumeMission()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().resumeFlight();
            return;
        }

        SendWaypointAction(WaypointMissionAction.RESUME);
    }
    
    /// <summary>
    /// The Control UI should call this function only
    /// Update the waypoint mission.
    /// </summary>
    public void UpdateMission()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().FlyNextWaypoint(true);
            return;
        }

        // Integrate dynamic waypoint system
        SendWaypointAction(WaypointMissionAction.STOP);
        StartMission();
    }
    
    /// <summary>
    /// The Control UI should call this function only
    /// Land the drone at the current position.
    /// </summary>
    public void LandDrone()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().flyHome();
            return;
        }

        ExecuteTask(DroneTask.LAND);
    }
    
    /// <summary>
    /// The Control UI should call this function only
    /// Command the drone to fly back to the home position.
    /// </summary>
    public void FlyHome()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().flyHome();
            return;
        }

        ExecuteTask(DroneTask.GO_HOME);
    }

    /// <summary>
    /// Public methods to query state variables of the drone
    /// The Informative UI should only query these methods
    /// </summary>

    /// <summary>
    /// State of control authority Unity interface has over drone
    /// </summary>
    public bool HasAuthority()
    {
        return has_authority;
    }
    
    /// <summary>
    /// Current drone flight status
    /// </summary>
    /// <returns></returns>
    public FlightStatus GetFlightStatus()
    {
        return flight_status;
    }

    /// <summary>
    /// Current attitude of the drone
    /// </summary>
    /// <returns></returns>
    public Quaternion GetAttitude()
    {
        return attitude;
    }

    /// <summary>
    /// Current GPS Position of the drone
    /// </summary>
    /// <returns></returns>
    public NavSatFixMsg GetGPSPosition()
    {
        return gps_position;
    }

    /// <summary>
    /// Current height of drone relative to take off height
    /// </summary>
    /// <returns></returns>
    public float GetHeightAboveTakeoff()
    {
        return relative_altitude;
    }

    /// <summary>
    /// Position of drone relative to set Local Position
    /// Not valid if Local Position has not been set
    /// </summary>
    /// <returns></returns>
    public Vector3 GetLocalPosition()
    {
        return local_position;
    }

    /// <summary>
    /// Current velocity of the drone
    /// </summary>
    /// <returns></returns>
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    /// <summary>
    /// Current angles of attached gimble
    /// </summary>
    /// <returns></returns>
    public Vector3 GetGimbleJointAngles()
    {
        return gimble_joint_angles;
    }

    /// <summary>
    /// Strength of GPS connection
    /// </summary>
    /// <returns></returns>
    public float GetGPSHealth()
    {
        return gps_health;
    }

    /// <summary>
    /// Home Latitude of the drone
    /// </summary>
    /// <returns></returns>
    public double GetHomeLat()
    {
        return droneHomeLat;
    }

    /// <summary>
    /// Home Longitude of the drone
    /// </summary>
    /// <returns></returns>
    public double GetHomeLong()
    {
        return droneHomeLong;
    }

    /// ROSTopicSubscriber Interface methods

    /// <summary>
    /// Parse received message from drone based on topic and perform required action
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="raw_msg"></param>
    /// <param name="parsed"></param>
    /// <returns></returns>
    public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
    {
        ROSBridgeMsg result = null;
        // Writing all code in here for now. May need to move out to separate handler functions when it gets too unwieldy.
        switch (topic)
        {
            case "/dji_sdk/attitude":
                QuaternionMsg attitudeMsg = (parsed == null) ? new QuaternionMsg(raw_msg["quaternion"]) : (QuaternionMsg)parsed;
                attitude = offset * (new Quaternion(attitudeMsg.GetX(), attitudeMsg.GetY(), attitudeMsg.GetZ(), attitudeMsg.GetW()));
                result = attitudeMsg;
                break;
            case "/dji_sdk/battery_state":
                battery_state = (parsed == null) ? new BatteryStateMsg(raw_msg) : (BatteryStateMsg)parsed;
                result = battery_state;
                break;
            case "/dji_sdk/flight_status":
                flight_status = (FlightStatus)(new UInt8Msg(raw_msg)).GetData();
                break;
            case "/dji_sdk/gimbal_angle":
                Vector3Msg gimbleAngleMsg = (parsed == null) ? new Vector3Msg(raw_msg["vector"]) : (Vector3Msg)parsed;
                gimble_joint_angles = new Vector3((float)gimbleAngleMsg.GetX(), (float)gimbleAngleMsg.GetY(), (float)gimbleAngleMsg.GetZ());
                result = gimbleAngleMsg;
                break;
            case "/dji_sdk/gps_health":
                gps_health = (parsed == null) ? (new UInt8Msg(raw_msg)).GetData() : ((UInt8Msg)parsed).GetData();
                break;
            case "/dji_sdk/gps_position":
                gps_position = (parsed == null) ? new NavSatFixMsg(raw_msg) : (NavSatFixMsg)parsed;
                result = gps_position;

                // Peru 6/14/20: Set drone home latitude and longitutde as first message from drone gps position.
                if (droneHomeSet == false)
                {
                    droneHomeLat = gps_position.GetLatitude();
                    droneHomeLong = gps_position.GetLongitude();
                    droneHomeSet = true;
                }

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
            default:
                Debug.LogError("Topic not implemented: " + topic);
                break;
        }
        return result;
    }

    /// <summary>
    /// Get ROS message type for a valid topic.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public string GetMessageType(string topic)
    {
        switch (topic)
        {
            case "/dji_sdk/attitude":
                return "geometry_msgs/QuaternionStamped";
            case "/dji_sdk/battery_state":
                return "sensor_msgs/BatteryState";
            case "/dji_sdk/flight_status":
                return "std_msgs/UInt8";
            case "/dji_sdk/gimbal_angle":
                return "geometry_msgs/Vector3Stamped";
            case "/dji_sdk/gps_health":
                return "std_msgs/UInt8";
            case "/dji_sdk/gps_position":
                return "sensor_msgs/NavSatFix";
            case "/dji_sdk/imu":
                return "sensor_msgs/Imu";
            case "/dji_sdk/rc":
                return "sensor_msgs/Joy";
            case "/dji_sdk/velocity":
                return "geometry_msgs/Vector3Stamped";
            case "/dji_sdk/height_above_takeoff":
                return "std_msgs/Float32";
            case "/dji_sdk/local_position":
                return "geometry_msgs/PointStamped";
        }
        Debug.LogError("Topic " + topic + " not registered.");
        return "";
    }

    /// <summary>
    /// Disconnect the ros connection, terminated via ROSManager
    /// </summary>
    public void DisconnectROSConnection()
    {
        ros.Disconnect();
    }


    /// <summary>
    /// Methods to execute service calls to the DJI SDK onboard the drone and corresponding methods to handle DJI SDK response
    /// All responses are currently printed out to the console. 
    /// Logical code implementation will be build as required.
    /// </summary>


    public void FetchDroneVersion()
    {
        string service_name = "dji_sdk/query_drone_version";
        ros.CallService(HandleDroneVersionResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    public void HandleDroneVersionResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Drone: {0} (Version {1})", response["hardware"].Value, response["version"].AsInt);
    }

    public void ActivateDrone()
    {
        string service_name = "/dji_sdk/activation";
        ros.CallService(HandleActivationResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    public void HandleActivationResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Activation {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    public void SetSDKControl(bool control)
    {
        string service_name = "/dji_sdk/sdk_control_authority";
        ros.CallService(HandleSetSDKControlResponse, service_name, string.Format("{0} {1}", client_id, service_name), string.Format("[{0}]", (control ? 1 : 0)));
        has_authority = control;
    }
    public void HandleSetSDKControlResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log(response.ToString());
        Debug.LogFormat("Control request {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
        //if (response["result"].AsBool == true)
        //{
        //    has_authority = requested_authority;
        //}
    }

    public void ChangeArmStatusTo(bool armed)
    {
        string service_name = "/dji_sdk/drone_arm_control";
        ros.CallService(HandleArmResponse, service_name, string.Format("{0} {1}", client_id, service_name), string.Format("[{0}]", (armed ? 1 : 0)));
    }
    public void HandleArmResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Arm/Disarm request {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    public void ExecuteTask(DroneTask task)
    {
        string service_name = "/dji_sdk/drone_task_control";
        ros.CallService(HandleTaskResponse, service_name, string.Format("{0} {1}", client_id, service_name), string.Format("[{0}]", (int)task));
    }
    public void HandleTaskResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Task request {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    public void SetLocalPosOriginToCurrentLocation()
    {
        string service_name = "/dji_sdk/set_local_pos_ref";
        ros.CallService(HandleSetLocalPosOriginResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    public void HandleSetLocalPosOriginResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Local position origin set {0}", (response["result"].AsBool ? "succeeded" : "failed"));
    }

    public void ExecuteCameraAction(CameraAction action)
    {
        string service_name = "/dji_sdk/camera_action";
        ros.CallService(HandleCameraActionResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", (int)action));
    }
    public void HandleCameraActionResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Camera action {0}", (response["result"].AsBool ? "succeeded" : "failed"));
    }

    public void FetchMissionStatus()
    {
        string service_name = "/dji_sdk/mission_status";
        ros.CallService(HandleMissionStatusResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    public void HandleMissionStatusResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Waypoint Count: {0}\nHotpoint Count: {1}", response["waypoint_mission_count"], response["hotpoint_mission_count"]);
    }

    public void UploadWaypointsTask(MissionWaypointTaskMsg task)
    {
        string service_name = "/dji_sdk/mission_waypoint_upload";
        ros.CallService(HandleUploadWaypointsTaskResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", task.ToYAMLString()));
    }
    public void HandleUploadWaypointsTaskResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Waypoint task upload {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);

        // Peru 6/10/20: Start flight upon completing upload
        if (response["result"].AsBool == true)
        {
            SendWaypointAction(WaypointMissionAction.START);
        }
        else
        {
            StartMission();
        }
    }

    public void SendWaypointAction(WaypointMissionAction action)
    {
        string service_name = "/dji_sdk/mission_waypoint_action";
        ros.CallService(HandleWaypointActionResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", action));
    }
    public void HandleWaypointActionResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Waypoint action {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    public void FetchCurrentWaypointMission()
    {
        string service_name = "/dji_sdk/mission_waypoint_getInfo";
        ros.CallService(HandleCurrentWaypointMissionResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    public void HandleCurrentWaypointMissionResponse(JSONNode response)
    {
        MissionWaypointTaskMsg waypoint_task = new MissionWaypointTaskMsg(response["values"]);
        Debug.LogFormat("Current waypoint mission: \n{0}", waypoint_task.ToYAMLString());
    }

    public void FetchWaypointSpeed()
    {
        string service_name = "/dji_sdk/mission_waypoint_getSpeed";
        ros.CallService(HandleWaypointSpeedResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    public void HandleWaypointSpeedResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Current waypoint speed: {0}", response["speed"].AsFloat);
    }

    public void SetWaypointSpeed(float speed)
    {
        string service_name = "/dji_sdk/mission_waypoint_setSpeed";
        ros.CallService(HandleSetWaypointSpeedResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", speed));
    }
    public void HandleSetWaypointSpeedResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Set waypoint speed {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    public void Subscribe240p(bool front_right, bool front_left, bool down_front, bool down_back)
    {
        string serviceName = "/dji_sdk/stereo_240p_subscription";
        string id = string.Format("{0} {1} subscribe", client_id, serviceName);
        string args = string.Format("[{0} {1} {2} {3} 0]", front_right ? 1 : 0, front_left ? 1 : 0, down_front ? 1 : 0, down_back ? 1 : 0);
        ros.CallService(HandleSubscribe240pResponse, serviceName, id, args);
    }
    public void HandleSubscribe240pResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe to 240p feeds " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void Unsubscribe240p()
    {
        string serviceName = "/dji_sdk/stereo_240p_subscription";
        string id = string.Format("{0} {1} unsubscribe", client_id, serviceName);
        ros.CallService(HandleUnsubscribe240pResponse, serviceName, id, "[0 0 0 0 1]");
    }
    public void HandleUnsubscribe240pResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe to 240p feeds " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void SubscribeDepthFront()
    {
        string serviceName = "/dji_sdk/stereo_depth_subscription";
        string id = string.Format("{0} {1} subscribe", client_id, serviceName);
        ros.CallService(HandleSubscribeDepthFrontResponse, serviceName, id, "[1 0]");
    }
    public void HandleSubscribeDepthFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe front depth feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void UnsubscribeDepthFront()
    {
        string serviceName = "/dji_sdk/stereo_depth_subscription";
        string id = string.Format("{0} {1} unsubscribe", client_id, serviceName);
        ros.CallService(HandleUnsubscribeDepthFrontResponse, serviceName, id, "[0 1]");
    }
    public void HandleUnsubscribeDepthFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe front depth feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void SubscribeVGAFront(bool use_20Hz)
    {
        string serviceName = "/dji_sdk/stereo_vga_subscription";
        string id = string.Format("{0} {1} subscribe", client_id, serviceName);
        ros.CallService(HandleSubscribeVGAFrontResponse, serviceName, id, string.Format("[{0} 1 0]", use_20Hz ? 0 : 1));
    }
    public void HandleSubscribeVGAFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe VGA front feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void UnsubscribeVGAFront()
    {
        string serviceName = "/dji_sdk/stereo_vga_subscription";
        string id = string.Format("{0} {1} unsubscribe", client_id, serviceName);
        ros.CallService(HandleUnsubscribeVGAFrontResponse, serviceName, id, "[0 0 1]");
    }
    public void HandleUnsubscribeVGAFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe VGA front feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void SubscribeFPV()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} subscribe FPV", client_id, serviceName);
        ros.CallService(HandleSubscribeFPVResponse, serviceName, id, "[0 1]");
    }
    public void HandleSubscribeFPVResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe FPV feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void UnsubscribeFPV()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} unsubscribe FPV", client_id, serviceName);
        ros.CallService(HandleUnsubscribeFPVResponse, serviceName, id, "[0 0]");
    }
    public void HandleUnsubscribeFPVResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe FPV feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void SubscribeMainCamera()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} subscribe MainCamera", client_id, serviceName);
        ros.CallService(HandleSubscribeMainCameraResponse, serviceName, id, "[1 1]");
    }
    public void HandleSubscribeMainCameraResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe MainCamera feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    public void UnsubscribeMainCamera()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} unsubscribe MainCamera", client_id, serviceName);
        ros.CallService(HandleUnsubscribeMainCameraResponse, serviceName, id, "[1 0]");
    }
    public void HandleUnsubscribeMainCameraResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe MainCamera feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

}
