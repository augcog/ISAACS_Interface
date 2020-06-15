using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;

using ISAACS;


public class M210_ROSDroneConnection : Matrice_ROSDroneConnection
{
    // TODO: Add/Remove functionality as required

    //TODO: Integrate Dynamic Waypoint system and interface
    /*
     
            // Dynamic waypoint system variables
        // We should have a system design discussion on where to have this code
        private static bool missionActive = false;
        private static bool waypointMissionUploading = false;
        private static bool inFlight = false;
        private static int nextWaypointID = 0;
        private static int currentWaypointID = 0;

    private void Update
    {

                // Peru: 5/24/2020 : Dynamic waypoint system
            //TODO: consider using invokes to not bombard the drone with queries
            //TODO: change the architecture to trigger an invoke instead of a boolean

            /// <summary>
            /// Query the drone to check if waypoint mission is finished uploading
            /// </summary>
            if (waypointMissionUploading)
            {
                //TODO: write ros service call to query drone status and trigger execute mission
                bool droneWaypointMissionUploaded = false;

                if (droneWaypointMissionUploaded)
                {
                    worldObject.GetComponent<ROSDroneConnection>().ExecuteMission();
                    waypointMissionUploading = false;
                    inFlight = true;
                }

            }

            /// <summary>
            /// Query the drone to check if waypoint mission is finished executing
            /// </summary>
            if (inFlight)
            {
                //TODO: write ros service call to query drone status and trigger next 
                bool droneWaypointMissionCompleted = false;

                if (droneWaypointMissionCompleted)
                {
                    inFlight = false;
                }
            }

            /// <summary>
            /// Create and execute next waypoint if mission is active
            /// </summary>
            if (missionActive && !inFlight)
            {
                UploadNextWaypointMission();
            }

    }
    /// <summary>
    /// Starts the drone flight. Uploads the first user-created waypoint.
    /// The drone will not start flying. It will store the mission, and wait for an execute mission call before flying.
    /// </summary>
    public static void StartDroneMission()
    {

        ArrayList waypoints = WorldProperties.selectedDrone.waypoints;

        /// Check if atleast one waypoint has been generated
        if (waypoints.Count < 2)
        {
            Debug.Log("ALERT: Not enough waypoints set to start mission");
            return;
        }

        // Create and execute waypoint mission of first two waypoints
        List<MissionWaypointMsg> missionMissionMsgList = new List<MissionWaypointMsg>();

        uint[] command_list = new uint[16];
        uint[] command_params = new uint[16];

        for (int i = 0; i < 16; i++)
        {
            command_list[i] = 0;
            command_params[i] = 0;
        }

        // Waypoint 0 is disregared as it is the act of taking off.
        Waypoint waypoint_0 = (Waypoint)waypoints[0];
        float prev_waypoint_x = waypoint_0.gameObjectPointer.transform.localPosition.x;
        float prev_waypoint_y = waypoint_0.gameObjectPointer.transform.localPosition.y;
        float prev_waypoint_z = waypoint_0.gameObjectPointer.transform.localPosition.z;


        // Waypoint 1 is the first waypoint mission.
        Waypoint waypoint_1 = (Waypoint)waypoints[1];
        float waypoint_x = waypoint_1.gameObjectPointer.transform.localPosition.x;
        float waypoint_y = waypoint_1.gameObjectPointer.transform.localPosition.y;
        float waypoint_z = waypoint_1.gameObjectPointer.transform.localPosition.z;

        double waypoint_ROS_x = WorldProperties.UnityXToLat(WorldProperties.droneHomeLat, waypoint_x);
        float waypoint_ROS_y = (waypoint_y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
        double waypoint_ROS_z = WorldProperties.UnityZToLong(WorldProperties.droneHomeLong, WorldProperties.droneHomeLat, waypoint_z);

        MissionWaypointMsg new_waypoint = new MissionWaypointMsg(waypoint_ROS_x, waypoint_ROS_z, waypoint_ROS_y, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

        // Generate dummy waypoint 1

        float dummy_1_waypoint_x = (waypoint_x + prev_waypoint_x) * (1 / 3);
        float dummy_1_waypoint_y = (waypoint_y + prev_waypoint_y) * (1 / 3);
        float dummy_1_waypoint_z = (waypoint_z + prev_waypoint_z) * (1 / 3);

        double dummy_1_waypoint_ROS_x = WorldProperties.UnityXToLat(WorldProperties.droneHomeLat, dummy_1_waypoint_x);
        float dummy_1_waypoint_ROS_y = (dummy_1_waypoint_y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
        double dummy_1_waypoint_ROS_z = WorldProperties.UnityZToLong(WorldProperties.droneHomeLong, WorldProperties.droneHomeLat, dummy_1_waypoint_z);

        MissionWaypointMsg dummy_1_waypoint = new MissionWaypointMsg(dummy_1_waypoint_ROS_x, dummy_1_waypoint_ROS_z, dummy_1_waypoint_ROS_y, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

        // Generate dummy waypoint 2

        float dummy_2_waypoint_x = (waypoint_x + prev_waypoint_x) * (2 / 3); ;
        float dummy_2_waypoint_y = (waypoint_y + prev_waypoint_y) * (2 / 3); ;
        float dummy_2_waypoint_z = (waypoint_z + prev_waypoint_z) * (2 / 3); ;

        double dummy_2_waypoint_ROS_x = WorldProperties.UnityXToLat(WorldProperties.droneHomeLat, dummy_2_waypoint_x);
        float dummy_2_waypoint_ROS_y = (dummy_2_waypoint_y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
        double dummy_2_waypoint_ROS_z = WorldProperties.UnityZToLong(WorldProperties.droneHomeLong, WorldProperties.droneHomeLat, dummy_2_waypoint_z);

        MissionWaypointMsg dummy_2_waypoint = new MissionWaypointMsg(dummy_2_waypoint_ROS_x, dummy_2_waypoint_ROS_z, dummy_2_waypoint_ROS_y, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

        Debug.Log("Adding waypoint mission: " + new_waypoint);

        // Create and upload waypoint mission

        missionMissionMsgList.Add(dummy_1_waypoint);
        missionMissionMsgList.Add(dummy_2_waypoint);
        missionMissionMsgList.Add(new_waypoint);

        MissionWaypointTaskMsg Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.POINT, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, missionMissionMsgList.ToArray());
        worldObject.GetComponent<ROSDroneConnection>().UploadMission(Task);

        // Start loop to check and publish waypoints
        nextWaypointID = 2;
        currentWaypointID = 1;
        missionActive = true;
        waypointMissionUploading = true;
    }

    /// <summary>
    /// Creates a mission task for the next user-created waypoint stored in WorldProperties and uploads it to the drone.
    /// The drone will not start flying. It will store the mission, and wait for an excecute mission call before flying.
    /// </summary>
    public static void UploadNextWaypointMission()
    {
        ArrayList waypoints = WorldProperties.selectedDrone.waypoints;

        /// Check if there is another waypoint
        if (waypoints.Count == nextWaypointID)
        {
            Debug.Log("ALERT: All waypoints successfully send");
            Debug.Log("ALERT: Drone is send home by default");

            missionActive = false;
            inFlight = false;
            worldObject.GetComponent<ROSDroneConnection>().GoHome();

            return;
        }

        // Create and execute waypoint mission of first two waypoints
        List<MissionWaypointMsg> missionMissionMsgList = new List<MissionWaypointMsg>();

        uint[] command_list = new uint[16];
        uint[] command_params = new uint[16];

        for (int i = 0; i < 16; i++)
        {
            command_list[i] = 0;
            command_params[i] = 0;
        }

        // Get the position of the last completed waypoint.
        Waypoint prev_waypoint = (Waypoint)waypoints[currentWaypointID];
        float prev_waypoint_x = prev_waypoint.gameObjectPointer.transform.localPosition.x;
        float prev_waypoint_y = prev_waypoint.gameObjectPointer.transform.localPosition.y;
        float prev_waypoint_z = prev_waypoint.gameObjectPointer.transform.localPosition.z;


        // Create a mission for the next user generated waypoint.
        Waypoint next_waypoint = (Waypoint)waypoints[nextWaypointID];
        float waypoint_x = next_waypoint.gameObjectPointer.transform.localPosition.x;
        float waypoint_y = next_waypoint.gameObjectPointer.transform.localPosition.y;
        float waypoint_z = next_waypoint.gameObjectPointer.transform.localPosition.z;

        double waypoint_ROS_x = WorldProperties.UnityXToLat(WorldProperties.droneHomeLat, waypoint_x);
        float waypoint_ROS_y = (waypoint_y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
        double waypoint_ROS_z = WorldProperties.UnityZToLong(WorldProperties.droneHomeLong, WorldProperties.droneHomeLat, waypoint_z);

        MissionWaypointMsg new_waypoint = new MissionWaypointMsg(waypoint_ROS_x, waypoint_ROS_z, waypoint_ROS_y, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

        // Generate dummy waypoint 1

        float dummy_1_waypoint_x = (waypoint_x + prev_waypoint_x) * (1 / 3);
        float dummy_1_waypoint_y = (waypoint_y + prev_waypoint_y) * (1 / 3);
        float dummy_1_waypoint_z = (waypoint_z + prev_waypoint_z) * (1 / 3);

        double dummy_1_waypoint_ROS_x = WorldProperties.UnityXToLat(WorldProperties.droneHomeLat, dummy_1_waypoint_x);
        float dummy_1_waypoint_ROS_y = (dummy_1_waypoint_y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
        double dummy_1_waypoint_ROS_z = WorldProperties.UnityZToLong(WorldProperties.droneHomeLong, WorldProperties.droneHomeLat, dummy_1_waypoint_z);

        MissionWaypointMsg dummy_1_waypoint = new MissionWaypointMsg(dummy_1_waypoint_ROS_x, dummy_1_waypoint_ROS_z, dummy_1_waypoint_ROS_y, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

        // Generate dummy waypoint 2

        float dummy_2_waypoint_x = (waypoint_x + prev_waypoint_x) * (2 / 3); ;
        float dummy_2_waypoint_y = (waypoint_y + prev_waypoint_y) * (2 / 3); ;
        float dummy_2_waypoint_z = (waypoint_z + prev_waypoint_z) * (2 / 3); ;

        double dummy_2_waypoint_ROS_x = WorldProperties.UnityXToLat(WorldProperties.droneHomeLat, dummy_2_waypoint_x);
        float dummy_2_waypoint_ROS_y = (dummy_2_waypoint_y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
        double dummy_2_waypoint_ROS_z = WorldProperties.UnityZToLong(WorldProperties.droneHomeLong, WorldProperties.droneHomeLat, dummy_2_waypoint_z);

        MissionWaypointMsg dummy_2_waypoint = new MissionWaypointMsg(dummy_2_waypoint_ROS_x, dummy_2_waypoint_ROS_z, dummy_2_waypoint_ROS_y, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

        Debug.Log("Adding waypoint mission: " + new_waypoint);

        // Create and upload waypoint mission

        missionMissionMsgList.Add(dummy_1_waypoint);
        missionMissionMsgList.Add(dummy_2_waypoint);
        missionMissionMsgList.Add(new_waypoint);

        MissionWaypointTaskMsg Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.POINT, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, missionMissionMsgList.ToArray());
        worldObject.GetComponent<ROSDroneConnection>().UploadMission(Task);

        // Start loop to check and publish waypoints
        currentWaypointID = nextWaypointID;
        nextWaypointID = nextWaypointID + 1;
        waypointMissionUploading = true;
    }

    /// <summary>
    /// Pause the drone flight
    /// </summary>
    public static void PauseDroneMission()
    {
        missionActive = false;
        inFlight = false;
        worldObject.GetComponent<ROSDroneConnection>().PauseMission();
    }

    /// <summary>
    /// Resume the drone flight
    /// </summary>
    public static void ResumeDroneMission()
    {
        missionActive = true;
        inFlight = false;
        worldObject.GetComponent<ROSDroneConnection>().ResumeMission();
    }

    */
}
