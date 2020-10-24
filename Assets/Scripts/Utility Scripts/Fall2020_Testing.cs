using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using ROSBridgeLib.sensor_msgs;
using System.IO;
using ROSBridgeLib.interface_msgs;

public class Fall2020_Testing : MonoBehaviour {


    /// <summary>
    /// This script is for all debugging using keyboard triggers. 
    /// Each string maps a key to the required function used for debugging.
    /// </summary>

    [Header("Initialize")]
    public string init = "0";

    [Header("Takeoff/Land Tests")]
    public string takeoffDrone = "i";
    public string landDrone = "o";

    [Header("Safe Mission Test (Hardcoded)")]
    public string uploadTestMission = "p";
    public string executeUploadedMission = "s";

    [Header("Safe Mission Test (User Set)")]
    public string viewSafeWaypointMission = "d";
    public string uploadUserMission = "f";

    [Header("Pause/Resume Mission Tests")]
    public string pauseMission = "g";
    public string resumeMission = "h";

    [Header("Drone/Sensor Selection Tests")]
    public string cycleDrone = "j";
    public string cycleSensor = "k";
    public string unsubscribe = "z";

    [Header("Dyanmic waypoint system Tests")]
    // TODO: Ensure waypoint array removes past waypoints.
    public string stopMission = "l";

    [Header("Drone Variable")]
    public Matrice_ROSDroneConnection rosDroneConnection;

    /// <summary>
    /// Set the rosDroneConnection to given argument
    /// </summary>
    /// <param name="new_ROSDroneConnection"></param>
    public void UpdateDrone(ROSDroneConnectionInterface new_ROSDroneConnection)
    {
        rosDroneConnection = (Matrice_ROSDroneConnection)new_ROSDroneConnection;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(init))
        {
            Drone drone = WorldProperties.SelectNextDrone();
            rosDroneConnection = (Matrice_ROSDroneConnection)drone.droneProperties.droneROSConnection;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            rosDroneConnection.SetSDKControl(false);
        }

        if (Input.GetKeyUp(takeoffDrone))
        {
            rosDroneConnection.ExecuteTask(Matrice_ROSDroneConnection.DroneTask.TAKEOFF);
        }

        if (Input.GetKeyUp(landDrone))
        {
            rosDroneConnection.ExecuteTask(Matrice_ROSDroneConnection.DroneTask.LAND);
        }

        if (Input.GetKeyUp(uploadTestMission))
        {
            uint[] command_list = new uint[16];
            uint[] command_params = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                command_list[i] = 0;
                command_params[i] = 0;
            }

            MissionWaypointMsg test_waypoint_1 = new MissionWaypointMsg(37.915701652f, -122.337967237f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
            MissionWaypointMsg test_waypoint_2 = new MissionWaypointMsg(37.915585270f, -122.338122805f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
            MissionWaypointMsg test_waypoint_3 = new MissionWaypointMsg(37.915457249f, -122.338015517f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

            Debug.Log("Check float accuracy here" + test_waypoint_1.ToYAMLString());

            MissionWaypointMsg[] test_waypoint_array = new MissionWaypointMsg[] { test_waypoint_1, test_waypoint_2, test_waypoint_3 };

            MissionWaypointTaskMsg test_Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.COORDINATED, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, test_waypoint_array);

            rosDroneConnection.UploadWaypointsTask(test_Task);
        }

        if (Input.GetKeyUp(executeUploadedMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.START);
        }

        if (Input.GetKeyUp(viewSafeWaypointMission))
        {


            // 37.915411
            // -122.338024

            // 37.915159, -122.337983


            // LEFT CORNER:  37.915188, -122.337988
            // RIGHT CORNER: 37.915262, -122.337984


            GameObject waypoint1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypoint1.name = "Waypoint 1";
            waypoint1.transform.parent = this.transform;
            waypoint1.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915701652, -122.337967237, 20));
            waypoint1.transform.localScale = Vector3.one * (0.05f);

            Debug.Log("SANITY CHECK GPS: " + WorldProperties.UnityCoordToGPSCoord(WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915701652, -122.337967237, 20))).Lat);
            Debug.Log("SANITY CHECK GPS: " + WorldProperties.UnityCoordToGPSCoord(WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915701652, -122.337967237, 20))).Lng);
            GameObject waypoint2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypoint2.name = "Waypoint 2";
            waypoint2.transform.parent = this.transform;
            waypoint2.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915585270, -122.338122805, 20));
            waypoint2.transform.localScale = Vector3.one * (0.05f);

            GameObject waypoint3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypoint3.name = "Waypoint 3";
            waypoint3.transform.parent = this.transform;
            waypoint3.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915457249, -122.338015517, 20));
            waypoint3.transform.localScale = Vector3.one * (0.05f);
        }

        if (Input.GetKeyUp(uploadUserMission))
        {
            rosDroneConnection.StartMission();
        }

        if (Input.GetKeyUp(pauseMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.PAUSE);
        }

        if (Input.GetKeyUp(resumeMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.RESUME);
        }

        if (Input.GetKeyDown(stopMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.STOP);
        }

        if (Input.GetKeyUp(cycleDrone))
        {
            Drone drone = WorldProperties.SelectNextDrone();
            rosDroneConnection = (Matrice_ROSDroneConnection)drone.droneProperties.droneROSConnection;
        }

        if (Input.GetKeyDown(cycleSensor))
        {
            WorldProperties.sensorManager.ShowNextSensor();
        }

        if (Input.GetKeyDown(unsubscribe))
        {
            ROSSensorConnectionInterface sensor = WorldProperties.sensorManager.getSelectedSensor();
            Dictionary<string, bool> subscriberDict = WorldProperties.sensorManager.getSubscriberDict();
            foreach (string topic in subscriberDict.Keys)
            {
                sensor.Unsubscribe(topic);
            }
        }
    }
}
