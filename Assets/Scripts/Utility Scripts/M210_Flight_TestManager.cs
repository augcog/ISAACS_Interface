using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using UnityEditor;
using System.IO;
using ISAACS;
using UnityEngine.UI;
using ROSBridgeLib.sensor_msgs;

public class M210_Flight_TestManager : MonoBehaviour
{

    /// <summary>
    /// This script is for all debugging using keyboard triggers. 
    /// Each string maps a key to the required function used for debugging.
    /// </summary>

    [Header("Initialize")]
    public string init = "0";

    [Header("Subscriber Tests")]
    public string viewHomeLat = "q";
    public string viewHomeLong = "1";
    public bool printLatLong = false;
    public bool printGPSHealth = false;

    [Header("Authority and KillSwitch Tests")]
    public string getAuthority = "e";
    public string hasAuthority = "r";
    public string killSwitch = "SPACE";

    [Header("Basic Motor Commands Test")]
    public string spinMotors = "y";
    public string stopMotors = "u";

    [Header("Takeoff/Land Tests")]
    public string takeoffDrone = "i";
    public string landDrone = "o";

    [Header("Safe Mission Test (Hardcoded)")]
    public string uploadTestMission = "p";
    public string missionInfo = "2";
    public string executeUploadedMission = "3";

    [Header("Safe Mission Test (User Set)")]
    public string viewSafeWaypointMission = "4";
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

    [Header("LAMP radiation type")]
    public string selectGamma = "z";
    public string selectNeutron = "x";

    [Header("106A Stuff")]
    public string testLocationDrop = "c";
    public string testGridSearch = "v";

    public string forward = "w";
    public string left = "a";
    public string back = "s";
    public string right = "d";

    public string changeStatus_toSearching = "b";
    public string changeStatus_toFound = "n";
    public UnityEngine.UI.Text SearchStatus;


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

        if (Input.GetKeyDown(viewHomeLat))
        {
            double homeLat = rosDroneConnection.GetHomeLat();
            Debug.LogFormat("Home Latitute set to {0}", homeLat);
        }

        if (Input.GetKeyDown(viewHomeLong))
        {
            double homeLong = rosDroneConnection.GetHomeLong();
            Debug.LogFormat("Home Longitute set to {0}", homeLong);
        }

        if (printLatLong)
        {
            NavSatFixMsg gpsPosition = rosDroneConnection.GetGPSPosition();
            Debug.LogFormat("Drone Lat,Long : {0},{1}", gpsPosition.GetLatitude(), gpsPosition.GetLongitude());
        }

        if (printGPSHealth)
        {
            float gpsHealth = rosDroneConnection.GetGPSHealth();
            Debug.LogFormat("GPS Health : {0}", gpsHealth);
        }

        if (Input.GetKeyUp(hasAuthority))
        {
            rosDroneConnection.HasAuthority();
        }

        if (Input.GetKeyUp(getAuthority))
        {
            rosDroneConnection.SetSDKControl(true);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            rosDroneConnection.SetSDKControl(false);
        }

        if (Input.GetKeyDown(spinMotors))
        {
            rosDroneConnection.ChangeArmStatusTo(true);
        }

        if (Input.GetKeyDown(stopMotors))
        {
            rosDroneConnection.ChangeArmStatusTo(false);
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

        if (Input.GetKeyUp(missionInfo))
        {
            rosDroneConnection.FetchMissionStatus();
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

            Debug.Log("SANITY CHECK GPS: " + WorldProperties.UnityCoordToGPSCoord( WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915701652, -122.337967237, 20))).Lat);
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

        if (Input.GetKeyDown(selectGamma))
        {
            WorldProperties.radiationDataType = WorldProperties.RadiationDataType.gamma;
        }

        if (Input.GetKeyDown(selectNeutron))
        {
            WorldProperties.radiationDataType = WorldProperties.RadiationDataType.neutron;
        }

        if (Input.GetKeyDown(testLocationDrop))
        {
            GameObject drone = WorldProperties.GetSelectedDrone().gameObjectPointer;
            Vector3 droneLocation = drone.transform.localPosition;

            Vector3 dropOffLocation = droneLocation + new Vector3(6.0f, 8.0f, 13.0f);
            GameObject dropOffPointer = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            dropOffPointer.transform.parent = drone.transform.parent;
            dropOffPointer.transform.localPosition = dropOffLocation;
            dropOffPointer.transform.localScale = new Vector3(0.5f, 1.0f, 0.5f);
        }

        if (Input.GetKeyDown(testGridSearch))
        {
            rosDroneConnection.droneProperties.droneClassPointer.TestGridSearch();
        }

        if (Input.GetKeyDown(forward))
        {
            rosDroneConnection.FlyForward();
        }

        if (Input.GetKeyDown(left))
        {
            rosDroneConnection.FlyLeft();
        }

        if (Input.GetKeyDown(back))
        {
            rosDroneConnection.FlyBack();
        }

        if (Input.GetKeyDown(right))
        {
            rosDroneConnection.FlyRight();
        }

        if (Input.GetKeyDown(changeStatus_toFound))
        {
            SearchStatus.text = "Search Status: Found";
        }

        if (Input.GetKeyDown(changeStatus_toSearching))
        {
            SearchStatus.text = "Search Status: Searching";
        }
    }
}
