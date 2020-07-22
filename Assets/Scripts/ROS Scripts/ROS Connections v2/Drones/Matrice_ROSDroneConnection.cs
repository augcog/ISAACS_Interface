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
    /// <para>
    /// Drone state variables and helper enums
    /// </para>    

    /// <summary>
    /// Current flight status of the drone
    /// </summary>
    public enum FlightStatus
    {
        ON_GROUND_STANDBY = 1,
        IN_AIR_STANDBY = 2,
        FLYING = 3,
        FLYING_HOME = 4,
        PAUSED_IN_AIR = 5,
        LANDING = 6,
        NULL = 7
    }

    /// <summary>
    /// Possible commands to update the drone mission with.
    /// </summary>
    public enum UpdateMissionAction
    {
        CONTINUE_MISSION = 0,
        UPDATE_CURRENT_MISSION = 1,
        END_AND_HOVER = 2
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
    /// The iindex of the waypoint the drone is currently flying to.
    /// </summary>
    private int currentWaypointID = 0;


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
    /// Status of ability to control drone via Unity ROS connection
    /// True: Commands send from Unity will be executed
    /// False: Commands send from Unity will be rejected as drone control authority is with another controller/operator
    /// </summary>
    bool has_authority = false;

    /// <summary>
    /// Battery state of the drone
    /// </summary>
    BatteryStateMsg battery_state = null;
    
    /// <summary>
    /// Current flight status of the drone
    /// </summary>
    FlightStatus flight_status = FlightStatus.ON_GROUND_STANDBY;

    /// <summary>
    /// Previous flight status of the drone
    /// </summary>
    FlightStatus prev_flight_status = FlightStatus.NULL;

    /// <summary>
    /// Reading of the 6 channels of the remote controller, published at 50 Hz.
    /// </summary>
    JoyMsg remote_controller_msg = null;

    /// <summary>
    /// Vehicle attitude is as quaternion for the rotation from Forward-Left-Up (FLU) body frame to East-North-Up (ENU) ground frame, published at 100 Hz.
    /// </summary>
    Quaternion attitude = Quaternion.identity;

    /// <summary>
    /// Home attitude of the drone
    /// </summary>
    Quaternion home_attitude = Quaternion.identity;

    /// <summary>
    /// Status of the home attitude
    /// </summary>
    bool home_attitude_set = false;

    /// <summary>
    /// Offset used to convert drone attitude to Unity axis.
    /// </summary>
    Quaternion offset = Quaternion.Euler(90, 180, 0);

    /// <summary>
    /// IMU data including raw gyro reading in FLU body frame, raw accelerometer reading in FLU body frame, and attitude estimation, 
    /// published at 100 Hz for M100, and 400 Hz for other platforms. 
    /// Note that raw accelerometer reading will give a Z direction 9.8 m/s2 when the drone is put on a level ground statically.
    /// </summary>
    IMUMsg imu = null;

    /// <summary>
    /// Current velocity of the drone
    /// </summary>
    Vector3 velocity = Vector3.zero;

    /// <summary>
    /// Height above takeoff location. It is only valid after drone is armed, when the flight controller has a reference altitude set.
    /// </summary>
    float relative_altitude = 0.0f;

    /// <summary>
    /// Local position in Cartesian ENU frame, of which the origin is set by the user by calling the /dji_sdk/set_local_pos_ref service. 
    /// Note that the local position is calculated from GPS position, so good GPS health is needed for the local position to be useful.
    /// </summary>
    Vector3 local_position = Vector3.zero;
    
    /// <summary>
    /// Current angles of gimbal
    /// </summary>
    Vector3 gimbal_joint_angles = Vector3.zero;
    
    /// <summary>
    /// Current gps health
    /// </summary>
    uint gps_health = 0;
    
    /// <summary>
    /// Current gps position
    /// </summary>
    NavSatFixMsg gps_position = null;
    
    /// <summary>
    /// Home position of the drone
    /// </summary>
    NavSatFixMsg home_position = null;

    /// <summary>
    /// Initilize drone home position if it hasn't been set yet
    /// </summary>
    bool home_position_set = false;

    /// <summary>
    /// Function called by ROSManager when Drone Gameobject is initilized to start the ROS connection with requested subscribers.
    /// </summary>
    /// <param name="uniqueID"> Unique identifier</param>
    /// <param name="droneIP"> Drone IP address for ROS connection</param>
    /// <param name="dronePort"> Drone Port value for ROS connection</param>
    /// <param name="droneSubscribers"> List of subscibers to connect to and display in the informative UI</param>
    /// <param name="simFlight"> Boolean value to active or deactive DroneFlightSim</param>
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
    /// The Control UI should call this function to start the mission
    /// Start the waypoint mission.
    /// </summary>
    public void StartMission()
    {
        // Integrate dynamic waypoint system
        Debug.Log("Starting drone mission");

        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().startMission();
            return;
        }

        switch (flight_status)
        {
            case FlightStatus.ON_GROUND_STANDBY:

                if (prev_flight_status == FlightStatus.NULL)
                {
                    currentWaypointID = 1;
                    flight_status = FlightStatus.FLYING;
                    prev_flight_status = FlightStatus.ON_GROUND_STANDBY;

                    List<MissionWaypointMsg> missionMsgList = new List<MissionWaypointMsg>();

                    uint[] command_list = new uint[16];
                    uint[] command_params = new uint[16];

                    for (int i = 0; i < 16; i++)
                    {
                        command_list[i] = 0;
                        command_params[i] = 0;
                    }

                    //If the following leads to a null pointer reference, then use instead: Drone currentlySelectedDrone = this.GetComponent<DroneProperties>().droneClassPointer;
                    Drone currentlySelectedDrone = WorldProperties.GetSelectedDrone();

                    // Start from 1 instead of 0, as takeoff is automatic.
                    for (int i = 1; i < currentlySelectedDrone.WaypointsCount(); i++)
                    {
                        Waypoint waypoint = currentlySelectedDrone.GetWaypoint(i);
                        Vector3 unityCoord = waypoint.gameObjectPointer.transform.localPosition;
                        GPSCoordinate rosCoord = WorldProperties.UnityCoordToGPSCoord(unityCoord);

                        MissionWaypointMsg new_waypoint = new MissionWaypointMsg(rosCoord.Lat, rosCoord.Lng, (float)rosCoord.Alt, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
                        Debug.Log("Adding waypoint at: " + new_waypoint);
                        missionMsgList.Add(new_waypoint);
                    }

                    MissionWaypointTaskMsg Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.POINT, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, missionMsgList.ToArray());
                    Debug.Log("Uploading waypoint mission");
                    UploadWaypointsTask(Task);

                }
                else
                {
                    UpdateMission(UpdateMissionAction.CONTINUE_MISSION);
                }

                break;

            case FlightStatus.IN_AIR_STANDBY:
                UpdateMission(UpdateMissionAction.CONTINUE_MISSION);
                break;

            case FlightStatus.PAUSED_IN_AIR:
                ResumeMission();
                break;

            case FlightStatus.LANDING:
            case FlightStatus.FLYING_HOME:
            case FlightStatus.FLYING:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }

    }

    /// <summary>
    /// The Control UI should call this function to pause mission.
    /// Pause an active mission.
    /// </summary>
    public void PauseMission()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().pauseFlight();
            return;
        }

        switch (flight_status)
        {
            case FlightStatus.ON_GROUND_STANDBY:
            case FlightStatus.IN_AIR_STANDBY:
            case FlightStatus.LANDING:
            case FlightStatus.FLYING_HOME:
            case FlightStatus.FLYING:
                prev_flight_status = flight_status;
                flight_status = FlightStatus.PAUSED_IN_AIR;
                SendWaypointAction(WaypointMissionAction.PAUSE);
                break;

            case FlightStatus.PAUSED_IN_AIR:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }


    }

    /// <summary>
    /// The Control UI should call this function to resume mission
    /// Resume a paused mission.
    /// </summary>
    public void ResumeMission()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().resumeFlight();
            return;
        }
        
        switch (flight_status)
        {
            case FlightStatus.ON_GROUND_STANDBY:
            case FlightStatus.IN_AIR_STANDBY:
                UpdateMission(UpdateMissionAction.CONTINUE_MISSION);
                break;

            case FlightStatus.PAUSED_IN_AIR:
                flight_status = prev_flight_status;
                prev_flight_status = FlightStatus.PAUSED_IN_AIR;
                SendWaypointAction(WaypointMissionAction.RESUME);
                break;

            case FlightStatus.FLYING:
            case FlightStatus.FLYING_HOME:
            case FlightStatus.LANDING:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }
    }


    /// <summary>
    /// The Control UI should call this function to update mission
    /// Update the waypoint mission.
    /// </summary>
    public void UpdateMission()
    {
        UpdateMission(UpdateMissionAction.UPDATE_CURRENT_MISSION);
    }

    /// <summary>
    /// Helper function for the UpdateMission function
    /// </summary>
    /// <param name="action"></param>
    public void UpdateMission(UpdateMissionAction action)
    {
        if (simDrone)
        {
            return;
        }

        switch (action)
        {
            case UpdateMissionAction.CONTINUE_MISSION:

                //If the following leads to a null pointer reference, then use instead: Drone currentlySelectedDrone = this.GetComponent<DroneProperties>().droneClassPointer;
                Drone currentlySelectedDrone = WorldProperties.GetSelectedDrone();

                if (currentWaypointID == currentlySelectedDrone.WaypointsCount())
                {
                    Debug.Log("Invalid Request: All waypoints have been flown");
                }

                prev_flight_status = flight_status;
                flight_status = FlightStatus.FLYING;

                List<MissionWaypointMsg> missionMsgList = new List<MissionWaypointMsg>();

                uint[] command_list = new uint[16];
                uint[] command_params = new uint[16];

                for (int i = 0; i < 16; i++)
                {
                    command_list[i] = 0;
                    command_params[i] = 0;
                }

                // Start from 1 instead of 0, as takeoff is automatic.
                for (int i = currentWaypointID; i < currentlySelectedDrone.WaypointsCount(); i++)
                {
                    currentWaypointID += 1;
                    Waypoint waypoint = currentlySelectedDrone.GetWaypoint(i);
                    Vector3 unityCoord = waypoint.gameObjectPointer.transform.localPosition;
                    GPSCoordinate rosCoord = WorldProperties.UnityCoordToGPSCoord(unityCoord);

                    MissionWaypointMsg new_waypoint = new MissionWaypointMsg(rosCoord.Lat, rosCoord.Lng, (float)rosCoord.Alt, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
                    Debug.Log("Adding waypoint at: " + new_waypoint);
                    missionMsgList.Add(new_waypoint);
                }

                MissionWaypointTaskMsg Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.POINT, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, missionMsgList.ToArray());
                Debug.Log("Uploading waypoint mission");
                UploadWaypointsTask(Task);

                break;

            case UpdateMissionAction.END_AND_HOVER:
                SendWaypointAction(WaypointMissionAction.STOP);
                break;

            case UpdateMissionAction.UPDATE_CURRENT_MISSION:
                SendWaypointAction(WaypointMissionAction.STOP);
                UpdateMission(UpdateMissionAction.CONTINUE_MISSION);
                break;

            default:
                Debug.Log("Invalid Mission update requested");
                break;
        }
    }
    
    /// <summary>
    /// The Control UI should call this function to land the drone
    /// Land the drone at the current position.
    /// </summary>
    public void LandDrone()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().landDrone();
            return;
        }

        ExecuteTask(DroneTask.LAND);
    }
    
    /// <summary>
    /// The Control UI should call this function to fly the drone home
    /// Command the drone to fly back to the home position.
    /// </summary>
    public void FlyHome()
    {
        if (simDrone)
        {
            this.GetComponent<DroneSimulationManager>().flyHome();
            return;
        }


        switch (flight_status)
        {
            case FlightStatus.ON_GROUND_STANDBY:
            case FlightStatus.IN_AIR_STANDBY:
            case FlightStatus.LANDING:
                ExecuteTask(DroneTask.GO_HOME);
                prev_flight_status = flight_status;
                flight_status = FlightStatus.FLYING_HOME;
                break;

            case FlightStatus.FLYING:
            case FlightStatus.PAUSED_IN_AIR:
                currentWaypointID -= 1;
                ExecuteTask(DroneTask.GO_HOME);
                prev_flight_status = flight_status;
                flight_status = FlightStatus.FLYING_HOME;
                break;

            case FlightStatus.FLYING_HOME:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }
    }

    /// <para>
    /// Public methods to query state variables of the drone
    /// The Informative UI should only query these methods
    /// </para>


    /// <summary>
    /// Get the value of a certain topic.
    /// To be used by the UI for further abstraction
    /// </summary>
    /// <param name="topic"></param>
    /// <returns> requested value as string </returns>
    public string GetValueByTopic(string topic)
    {
        try
        {
            switch (topic)
            {
                case "/dji_sdk/attitude":
                case "attitude":
                    return attitude.ToString();
                case "/dji_sdk/battery_state":
                case "battery_state":
                    return battery_state.ToString();
                case "/dji_sdk/flight_status":
                case "flight_status":
                    return flight_status.ToString();
                case "/dji_sdk/gimbal_angle":
                case "gimbal_angle":
                    return gimbal_joint_angles.ToString();
                case "/dji_sdk/gps_health":
                case "gps_health":
                    return gps_health.ToString();
                case "/dji_sdk/gps_position":
                case "gps_position":
                case "/dji_sdk/rtk_position":
                case "rtk_position":
                    return gps_position.ToString();
                case "/dji_sdk/imu":
                case "imu":
                    return imu.ToString();
                case "/dji_sdk/rc":
                case "rc":
                    return remote_controller_msg.ToString();
                case "/dji_sdk/velocity":
                case "velocity":
                    return velocity.ToString();
                case "/dji_sdk/height_above_takeoff":
                case "height_above_takeoff":
                    return relative_altitude.ToString();
                case "/dji_sdk/local_position":
                case "local_position":
                    return local_position.ToString();
                default:
                    Debug.LogError("Topic " + topic + " not registered.");
                    return "INVALID TOPIC";
            }
        }
        catch (Exception e)
        {
            print("Error: " + e);
            return " NO DATA ";
        }

    }


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
    /// Home attitude
    /// </summary>
    /// <returns></returns>
    public Quaternion GetHomeAttitude()
    {
        return home_attitude;
    }

    /// <summary>
    /// Current IMU readings
    /// </summary>
    /// <returns></returns>
    public IMUMsg GetIMU()
    {
        return imu;
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
    /// Current angles of attached gimbal
    /// </summary>
    /// <returns></returns>
    public Vector3 GetGimbalJointAngles()
    {
        return gimbal_joint_angles;
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
        return home_position.GetLatitude();
    }

    /// <summary>
    /// Home Longitude of the drone
    /// </summary>
    /// <returns></returns>
    public double GetHomeLong()
    {
        return home_position.GetLongitude();
    }

    /// <summary>
    /// Home coordinates of the drone
    /// </summary>
    /// <returns></returns>
    public NavSatFixMsg GetHome()
    {
        return home_position;
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
                flight_status = (FlightStatus)(new UInt8Msg(raw_msg)).GetData();
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
                }

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
            case "/dji_sdk/rtk_position":
                return "sensor_msgs/NavSatFix";
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
    /// Localize all the sensors attached to the drone when both orientation and gps positions are set
    /// </summary>
    public void LocalizeSensors()
    {
        if (home_attitude_set == false || home_position_set == false)
        {
            return;
        }

        Quaternion orientation = home_attitude; 
        Vector3 position = WorldProperties.ROSCoordToUnityCoord(home_position);
        this.GetComponent<DroneProperties>().LocalizeSensors(position, orientation);
    }

    /// <para>
    /// Methods to execute service calls to the DJI SDK onboard the drone and corresponding methods to handle DJI SDK response
    /// All responses are currently printed out to the console. 
    /// Logical code implementation will be build as required.
    /// </para>

    /// <summary>
    /// Query drone version
    /// </summary>
    public void FetchDroneVersion()
    {
        string service_name = "dji_sdk/query_drone_version";
        Debug.LogFormat("ROS Call: {0} {1}", client_id, service_name);
        ros.CallService(HandleDroneVersionResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    /// <summary>
    /// Parse drone query response.
    /// </summary>
    /// <param name="response"></param>
    public void HandleDroneVersionResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Drone: {0} (Version {1})", response["hardware"].Value, response["version"].AsInt);
    }

    /// <summary>
    /// Activate drone
    /// </summary>
    public void ActivateDrone()
    {
        string service_name = "/dji_sdk/activation";
        Debug.LogFormat("ROS Call: {0} {1}", client_id, service_name);
        ros.CallService(HandleActivationResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    /// <summary>
    /// Parse drone activation response.
    /// </summary>
    /// <param name="response"></param>
    public void HandleActivationResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Activation {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    /// <summary>
    /// Obtain or relinquish control over drone
    /// </summary>
    /// <param name="control"></param>
    public void SetSDKControl(bool control)
    {
        string service_name = "/dji_sdk/sdk_control_authority";
        Debug.LogFormat("ROS Call: {0} {1}  Arguments: {2}", client_id, service_name, control);
        ros.CallService(HandleSetSDKControlResponse, service_name, string.Format("{0} {1}", client_id, service_name), string.Format("[{0}]", (control ? 1 : 0)));
        has_authority = control;
    }
    /// <summary>
    /// Parse SDK control response
    /// </summary>
    /// <param name="response"></param>
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

    /// <summary>
    /// Command a drone arm to perform a task
    /// </summary>
    /// <param name="armed"></param>
    public void ChangeArmStatusTo(bool armed)
    {
        string service_name = "/dji_sdk/drone_arm_control";
        Debug.LogFormat("ROS Call: {0} {1}  Arguments: {2}", client_id, service_name, armed);
        ros.CallService(HandleArmResponse, service_name, string.Format("{0} {1}", client_id, service_name), string.Format("[{0}]", (armed ? 1 : 0)));
    }
    /// <summary>
    /// Parse drone arm status response
    /// </summary>
    /// <param name="response"></param>
    public void HandleArmResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Arm/Disarm request {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    /// <summary>
    /// Command drone to execute task
    /// </summary>
    /// <param name="task"></param>
    public void ExecuteTask(DroneTask task)
    {
        string service_name = "/dji_sdk/drone_task_control";
        Debug.LogFormat("ROS Call: {0} {1}  Arguments: {2}", client_id, service_name, task);
        ros.CallService(HandleTaskResponse, service_name, string.Format("{0} {1}", client_id, service_name), string.Format("[{0}]", (int)task));
    }
    /// <summary>
    /// Parse drone task command response
    /// </summary>
    /// <param name="response"></param>
    public void HandleTaskResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Task request {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    /// <summary>
    /// Set Local Position origion of the drone
    /// </summary>
    public void SetLocalPosOriginToCurrentLocation()
    {
        string service_name = "/dji_sdk/set_local_pos_ref";
        Debug.LogFormat("ROS Call: {0} {1}", client_id, service_name);
        ros.CallService(HandleSetLocalPosOriginResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    /// <summary>
    /// Parse response of setting local drone position
    /// </summary>
    /// <param name="response"></param>
    public void HandleSetLocalPosOriginResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Local position origin set {0}", (response["result"].AsBool ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Execute camera action
    /// </summary>
    /// <param name="action"></param>
    public void ExecuteCameraAction(CameraAction action)
    {
        string service_name = "/dji_sdk/camera_action";
        Debug.LogFormat("ROS Call: {0} {1}  Arguments: {2}", client_id, service_name, action);
        ros.CallService(HandleCameraActionResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", (int)action));
    }
    /// <summary>
    /// Parse response of executing camera action
    /// </summary>
    /// <param name="response"></param>
    public void HandleCameraActionResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Camera action {0}", (response["result"].AsBool ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Query current mission status
    /// </summary>
    public void FetchMissionStatus()
    {
        string service_name = "/dji_sdk/mission_status";
        Debug.LogFormat("ROS Call: {0} {1} ", client_id, service_name);
        ros.CallService(HandleMissionStatusResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    /// <summary>
    /// Parse mission status query response
    /// </summary>
    /// <param name="response"></param>
    public void HandleMissionStatusResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Waypoint Count: {0}\nHotpoint Count: {1}", response["waypoint_mission_count"], response["hotpoint_mission_count"]);
    }

    /// <summary>
    /// Upload waypoint mission task
    /// </summary>
    /// <param name="task"></param>
    public void UploadWaypointsTask(MissionWaypointTaskMsg task)
    {
        string service_name = "/dji_sdk/mission_waypoint_upload";
        Debug.LogFormat("ROS Call: {0} {1}  Arguments: {2}", client_id, service_name, task);
        ros.CallService(HandleUploadWaypointsTaskResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", task.ToYAMLString()));
    }
    /// <summary>
    /// Parse waypoint mission taks upload response
    /// </summary>
    /// <param name="response"></param>
    public void HandleUploadWaypointsTaskResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Waypoint task upload {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);

        // Start flight upon completing upload        
        if (response["result"].AsBool == true)
        {
            Debug.Log("Executing mission");
            
            // @Eric,Nitzan: Uncomment as needed.
            //SendWaypointAction(WaypointMissionAction.START);
        }
        else
        {
            Debug.Log("Mission upload failed");
            //StartMission();
        }
        
    }

    /// <summary>
    /// Send waypoint action command
    /// </summary>
    /// <param name="action"></param>
    public void SendWaypointAction(WaypointMissionAction action)
    {
        string service_name = "/dji_sdk/mission_waypoint_action";
        Debug.LogFormat("ROS Call: {0} {1}  Arguments: {2}", client_id, service_name, action);
        ros.CallService(HandleWaypointActionResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", (int)action));
    }
    /// <summary>
    /// Parse response to sent waypoint action command
    /// </summary>
    /// <param name="response"></param>
    public void HandleWaypointActionResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Waypoint action {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    /// <summary>
    /// Query current waypoint mission
    /// </summary>
    public void FetchCurrentWaypointMission()
    {
        string service_name = "/dji_sdk/mission_waypoint_getInfo";
        Debug.LogFormat("ROS Call: {0} {1}", client_id, service_name);
        ros.CallService(HandleCurrentWaypointMissionResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    /// <summary>
    /// Parse current waypoint mission query response
    /// </summary>
    /// <param name="response"></param>
    public void HandleCurrentWaypointMissionResponse(JSONNode response)
    {
        MissionWaypointTaskMsg waypoint_task = new MissionWaypointTaskMsg(response["values"]);
        Debug.LogFormat("Current waypoint mission: \n{0}", waypoint_task.ToYAMLString());
    }

    /// <summary>
    /// Query waypoint velocity
    /// </summary>
    public void FetchWaypointSpeed()
    {
        string service_name = "/dji_sdk/mission_waypoint_getSpeed";
        Debug.LogFormat("ROS Call: {0} {1}", client_id, service_name);
        ros.CallService(HandleWaypointSpeedResponse, service_name, string.Format("{0} {1}", client_id, service_name));
    }
    /// <summary>
    /// Parse waypoint velocity query response
    /// </summary>
    /// <param name="response"></param>
    public void HandleWaypointSpeedResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Current waypoint speed: {0}", response["speed"].AsFloat);
    }

    /// <summary>
    /// Set waypoint mission velocity
    /// </summary>
    /// <param name="speed"></param>
    public void SetWaypointSpeed(float speed)
    {
        string service_name = "/dji_sdk/mission_waypoint_setSpeed";
        Debug.LogFormat("ROS Call: {0} {1}  Arguments: {2}", client_id, service_name, speed);
        ros.CallService(HandleSetWaypointSpeedResponse, service_name, string.Format("{0} {1}", client_id, service_name), args: string.Format("[{0}]", speed));
    }
    /// <summary>
    /// Parse response of setting waypoint mission velocity command
    /// </summary>
    /// <param name="response"></param>
    public void HandleSetWaypointSpeedResponse(JSONNode response)
    {
        response = response["values"];
        Debug.LogFormat("Set waypoint speed {0} (ACK: {1})", (response["result"].AsBool ? "succeeded" : "failed"), response["ack_data"].AsInt);
    }

    /// <summary>
    /// Subscribe to specified 240p camera's
    /// </summary>
    /// <param name="front_right"></param>
    /// <param name="front_left"></param>
    /// <param name="down_front"></param>
    /// <param name="down_back"></param>
    public void Subscribe240p(bool front_right, bool front_left, bool down_front, bool down_back)
    {
        string serviceName = "/dji_sdk/stereo_240p_subscription";
        string id = string.Format("{0} {1} subscribe", client_id, serviceName);
        string args = string.Format("[{0} {1} {2} {3} 0]", front_right ? 1 : 0, front_left ? 1 : 0, down_front ? 1 : 0, down_back ? 1 : 0);
        ros.CallService(HandleSubscribe240pResponse, serviceName, id, args);
    }
    /// <summary>
    /// Parse camera stream responses
    /// </summary>
    /// <param name="response"></param>
    public void HandleSubscribe240pResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe to 240p feeds " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Unsubscribe from camera stream
    /// </summary>
    public void Unsubscribe240p()
    {
        string serviceName = "/dji_sdk/stereo_240p_subscription";
        string id = string.Format("{0} {1} unsubscribe", client_id, serviceName);
        ros.CallService(HandleUnsubscribe240pResponse, serviceName, id, "[0 0 0 0 1]");
    }
    /// <summary>
    /// Parse response for unsubscription from camera stream request
    /// </summary>
    /// <param name="response"></param>
    public void HandleUnsubscribe240pResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe to 240p feeds " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Subscribe to front depth camera
    /// </summary>
    public void SubscribeDepthFront()
    {
        string serviceName = "/dji_sdk/stereo_depth_subscription";
        string id = string.Format("{0} {1} subscribe", client_id, serviceName);
        ros.CallService(HandleSubscribeDepthFrontResponse, serviceName, id, "[1 0]");
    }
    /// <summary>
    /// Parse response to front depth camera subscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleSubscribeDepthFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe front depth feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Unsubscribe to front depth camera
    /// </summary>
    public void UnsubscribeDepthFront()
    {
        string serviceName = "/dji_sdk/stereo_depth_subscription";
        string id = string.Format("{0} {1} unsubscribe", client_id, serviceName);
        ros.CallService(HandleUnsubscribeDepthFrontResponse, serviceName, id, "[0 1]");
    }
    /// <summary>
    /// Parse response to front depth camera unsubscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleUnsubscribeDepthFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe front depth feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Subscribe to front VGA camera
    /// </summary>
    public void SubscribeVGAFront(bool use_20Hz)
    {
        string serviceName = "/dji_sdk/stereo_vga_subscription";
        string id = string.Format("{0} {1} subscribe", client_id, serviceName);
        ros.CallService(HandleSubscribeVGAFrontResponse, serviceName, id, string.Format("[{0} 1 0]", use_20Hz ? 0 : 1));
    }
    /// <summary>
    /// Parse response to front VGA camera subscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleSubscribeVGAFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe VGA front feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Unsubscribe to front VGA camera
    /// </summary>
    public void UnsubscribeVGAFront()
    {
        string serviceName = "/dji_sdk/stereo_vga_subscription";
        string id = string.Format("{0} {1} unsubscribe", client_id, serviceName);
        ros.CallService(HandleUnsubscribeVGAFrontResponse, serviceName, id, "[0 0 1]");
    }
    /// <summary>
    /// Parse response to front VGA camera unsubscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleUnsubscribeVGAFrontResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe VGA front feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Subscribe to FPV camera
    /// </summary>
    public void SubscribeFPV()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} subscribe FPV", client_id, serviceName);
        ros.CallService(HandleSubscribeFPVResponse, serviceName, id, "[0 1]");
    }
    /// <summary>
    /// Parse response to FPV camera subscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleSubscribeFPVResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe FPV feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Unsubscribe to FPV camera
    /// </summary>
    public void UnsubscribeFPV()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} unsubscribe FPV", client_id, serviceName);
        ros.CallService(HandleUnsubscribeFPVResponse, serviceName, id, "[0 0]");
    }
    /// <summary>
    /// Parse response to FPV camera unsubscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleUnsubscribeFPVResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe FPV feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Subscribe to main camera
    /// </summary>
    public void SubscribeMainCamera()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} subscribe MainCamera", client_id, serviceName);
        ros.CallService(HandleSubscribeMainCameraResponse, serviceName, id, "[1 1]");
    }
    /// <summary>
    /// Parse response to main camera subscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleSubscribeMainCameraResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Subscribe MainCamera feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

    /// <summary>
    /// Unsubscribe to main camera
    /// </summary>
    public void UnsubscribeMainCamera()
    {
        string serviceName = "/dji_sdk/setup_camera_stream";
        string id = string.Format("{0} {1} unsubscribe MainCamera", client_id, serviceName);
        ros.CallService(HandleUnsubscribeMainCameraResponse, serviceName, id, "[1 0]");
    }
    /// <summary>
    /// Parse response to main camera unsubscription command
    /// </summary>
    /// <param name="response"></param>
    public void HandleUnsubscribeMainCameraResponse(JSONNode response)
    {
        response = response["values"];
        Debug.Log("Unsubscribe MainCamera feed " + ((response["result"].AsBool) ? "succeeded" : "failed"));
    }

}
