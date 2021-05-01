using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using ROSBridgeLib.sensor_msgs;
using System.IO;
using ROSBridgeLib.interface_msgs;

public class Spring_2020Testing : MonoBehaviour
{


    /// <summary>
    /// This script is for all debugging using keyboard triggers. 
    /// Each string maps a key to the required function used for debugging.
    /// </summary>

    [Header("Initialize")]
    public string initTestDrones = "0";

    [Header("Drone/Sensor Selection Tests")]
    public string cycleDrone = "q";
    public string cycleSensor = "w";

    [Header("Drone Command Tests")]
    public string landDrone = "t";
    public string pauseMission = "u";
    public string resumeMission = "i";
    public string stopMission = "s";
    public string flyHome = "o";

    [Header("Upload Missions (Hardcoded)")]
    public string setTestMission = "p";
    public string uploadUserMission = "a";

    [Header("Run Missions (User Set)")]
    public string runMission = "r";
    public string clearMission = "c";

    [Header("Dyanmic waypoint system Tests")]
    // TODO: Ensure waypoint array removes past waypoints.


    [Header("Drone Variable")]
    public static Drone selectedDrone = WorldProperties.GetSelectedDrone();
    ROSDroneConnectionInterface droneROSConnection = selectedDrone.droneProperties.droneROSConnection;

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(initTestDrones))
        {
            // Call server connection create drone function.
            //ServerConnections.DroneInformation test_drone_1 = new ServerConnections.DroneInformation("jazzy", 1);
            ////ServerConnections.DroneInformation test_drone_2 = new ServerConnections.DroneInformation("banana", 2);

            //ServerConnections.InstantiateDrone(test_drone_1);
            //ServerConnections.InstantiateDrone(test_drone_2);
        }

        if (Input.GetKeyUp(cycleDrone))
        {
            selectedDrone = WorldProperties.SelectNextDrone();
        }

        if (Input.GetKeyDown(cycleSensor))
        {
            WorldProperties.sensorManager.ShowNextSensor();
        }

        if (Input.GetKeyUp(landDrone))
        {
            droneROSConnection.LandDrone();
        }

        if (Input.GetKeyUp(pauseMission))
        {
            droneROSConnection.PauseMission();
        }

        if (Input.GetKeyUp(resumeMission))
        {
            droneROSConnection.ResumeMission();
        }

        if (Input.GetKeyUp(clearMission))
        {
            selectedDrone.DeleteAllWaypoints();
        }


        if (Input.GetKeyUp(stopMission))
        {
            droneROSConnection.LandDrone();
        }

        if (Input.GetKeyUp(flyHome))
        {
            droneROSConnection.FlyHome();
        }

        if (Input.GetKeyUp(setTestMission))
        {
            Vector3 drone_pos = selectedDrone.gameObjectPointer.transform.localPosition;
            Vector3 test_waypoint_1 = new Vector3(drone_pos.x + 3.0f, drone_pos.y + 4.0f, drone_pos.z);
            Vector3 test_waypoint_2 = new Vector3(drone_pos.x + 4.0f, drone_pos.y + 4.0f, drone_pos.z + 2.0f);
            Vector3 test_waypoint_3 = new Vector3(drone_pos.x + 1.0f, drone_pos.y + 4.0f, drone_pos.z + 4.0f);

            selectedDrone.AddWaypoint(test_waypoint_1);
            selectedDrone.AddWaypoint(test_waypoint_2);
            selectedDrone.AddWaypoint(test_waypoint_3);
        }

        if (Input.GetKeyUp(uploadUserMission))
        {
            droneROSConnection.StartMission();
        }

        if (Input.GetKeyDown(runMission))
        {
            droneROSConnection.StartMission();
        }



    }
}