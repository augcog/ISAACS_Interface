using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using UnityEditor;
using System.IO;
using ISAACS;


public class DebuggingManager : MonoBehaviour {

    /// <summary>
    /// This script is for all debugging using keyboard triggers. 
    /// Each string maps a key to the required function used for debugging.
    /// </summary>


    [Header("Radiation Buttons")]
    public string subscribeSurfacePointCloud = "6";
    public string subscribeColorized0 = "0";
    public string subscribeColorized1 = "1";
    public string subscribeColorized2 = "2";
    public string subscribeColorized3 = "3";
    public string subscribeColorized4 = "4";
    public string subscribeColorized5 = "5";

    [Header("Drone Commands")]
    public string getAuthority = "q";
    public string getVersion = "w";
    public string takeoffDrone= "e";
    public string landDrone = "r";
    public string uploadTestMission = "t";
    public string missionInfo = "y";
    public string executeUploadedMission = "u";

    [Header("MapBox Commands")]
    public string initMapBox = "i";

    [Header("World Properties")]
    public string displaySensorList = "o";

    [Header("Misc.")]
    public string initTestDrone = "p";
    public string hardcodedWaypoints = "a";
    public static double droneHomeLat = 37.91532757;
    public static double droneHomeLong = -122.33805556;
    public static float droneHomeAlt = 10.0f;
    public static bool droneInitialPositionSet = false;

    [Header("Application Variables")]
    public Drone selectedDrone;
    public GameObject selectedSensor;
    //public ROSSensorConnection rosSensorConnection;

    // Update is called once per frame
    void Update () {

        if (Input.GetKeyUp(subscribeColorized0))
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            lampSensor_ROS.LampSubscribe_Colorized_0();
        }

        if (Input.GetKeyUp(subscribeColorized1))
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            lampSensor_ROS.LampSubscribe_Colorized_1();
        }

        if (Input.GetKeyUp(subscribeColorized2))
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            lampSensor_ROS.LampSubscribe_Colorized_2();
        }

        if (Input.GetKeyUp(subscribeColorized3))
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            lampSensor_ROS.LampSubscribe_Colorized_3();
        }

        if (Input.GetKeyUp(subscribeColorized4))
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            lampSensor_ROS.LampSubscribe_Colorized_4();
        }

        if (Input.GetKeyUp(subscribeColorized5))
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            lampSensor_ROS.LampSubscribe_Colorized_5();
        }

        if (Input.GetKeyUp(subscribeSurfacePointCloud))
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            lampSensor_ROS.LampSubscribe_SurfacePointcloud();
        }
        
        if (Input.GetKeyUp(getAuthority))
        {
            Drone selectedDrone = WorldProperties.GetSelectedDrone();
            Matrice_ROSDroneConnection droneROSConnection = (Matrice_ROSDroneConnection) selectedDrone.droneProperties.droneROSConnection;
            droneROSConnection.HasAuthority();

        }

        if (Input.GetKeyUp(getVersion))
        {
            Drone selectedDrone = WorldProperties.GetSelectedDrone();
            Matrice_ROSDroneConnection droneROSConnection = (Matrice_ROSDroneConnection)selectedDrone.droneProperties.droneROSConnection;
            droneROSConnection.FetchDroneVersion();
        }

        if (Input.GetKeyUp(takeoffDrone))
        {
            Drone selectedDrone = WorldProperties.GetSelectedDrone();
            Matrice_ROSDroneConnection droneROSConnection = (Matrice_ROSDroneConnection)selectedDrone.droneProperties.droneROSConnection;
            droneROSConnection.ExecuteTask(Matrice_ROSDroneConnection.DroneTask.TAKEOFF);
        }

        if (Input.GetKeyUp(landDrone))
        {
            Drone selectedDrone = WorldProperties.GetSelectedDrone();
            ROSDroneConnectionInterface droneROSConnection = selectedDrone.droneProperties.droneROSConnection;
            droneROSConnection.LandDrone();
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

            MissionWaypointTaskMsg test_Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.RETURN_TO_HOME, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.COORDINATED, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, test_waypoint_array);


            Drone selectedDrone = WorldProperties.GetSelectedDrone();
            Matrice_ROSDroneConnection droneROSConnection = (Matrice_ROSDroneConnection)selectedDrone.droneProperties.droneROSConnection;
            droneROSConnection.UploadWaypointsTask(test_Task);
        }

        if (Input.GetKeyUp(missionInfo))
        {
            Drone selectedDrone = WorldProperties.GetSelectedDrone();
            Matrice_ROSDroneConnection droneROSConnection = (Matrice_ROSDroneConnection)selectedDrone.droneProperties.droneROSConnection;
            droneROSConnection.FetchMissionStatus();
        }

        if (Input.GetKeyUp(executeUploadedMission))
        {
            Drone selectedDrone = WorldProperties.GetSelectedDrone();
            Matrice_ROSDroneConnection droneROSConnection = (Matrice_ROSDroneConnection)selectedDrone.droneProperties.droneROSConnection;
            droneROSConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.START);
        }

        if (Input.GetKeyUp(initMapBox))
        {
            Debug.Log("Mapbox deactivated");
            //MapInteractions mapInteractions = worldProperties.GetComponent<MapInteractions>();
            //mapInteractions.InitializeCityMap();
        }

        if (Input.GetKeyUp(initTestDrone))
        {
            NewDrone();
        }

        if (Input.GetKeyUp(hardcodedWaypoints))
        {
            droneInitialPositionSet = true;
        }

        if (Input.GetKeyUp(displaySensorList))
        {
            Debug.Log(WorldProperties.GetSensorDict().Count.ToString());
        }
    }

    /// <summary>
    /// Creates a new drone
    /// </summary>
    public void NewDrone()
    {
        if (!GameObject.FindWithTag("Drone"))
        {

            Debug.Log("Initializing drone");
            Drone newDrone = new Drone( WorldProperties.worldObject.transform.position);
            WorldProperties.UpdateSelectedDrone(newDrone);
        }
    }

}
