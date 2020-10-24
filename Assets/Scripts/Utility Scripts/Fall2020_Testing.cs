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
    public string uploadTestMission = "p";
    public string uploadUserMission = "a";

    [Header("Run Missions (User Set)")]
    public string runMission = "s";

    [Header("Dyanmic waypoint system Tests")]
    // TODO: Ensure waypoint array removes past waypoints.


    [Header("Drone Variable")]
    public Drone_v2 drone;

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(initTestDrones))
        {
            // Call server connection create drone function.
            ServerConnections.DroneInformation test_drone_1 = new ServerConnections.DroneInformation("jazzy", 0);
            ServerConnections.DroneInformation test_drone_2 = new ServerConnections.DroneInformation("banana", 1);

            ServerConnections.InstantiateDrone(test_drone_1);
            ServerConnections.InstantiateDrone(test_drone_2);
        }

        if (Input.GetKeyUp(cycleDrone))
        {
            drone = WorldProperties.SelectNextDrone();
        }

        if (Input.GetKeyDown(cycleSensor))
        {
            WorldProperties.sensorManager.ShowNextSensor();
        }

        if (Input.GetKeyUp(landDrone))
        {
            drone.landDrone();
        }

        if (Input.GetKeyUp(pauseMission))
        {
            drone.pauseMission();
        }

        if (Input.GetKeyUp(resumeMission))
        {
            drone.resumeMission();
        }

        if (Input.GetKeyUp(stopMission))
        {
            drone.stopMission();
        }

        if (Input.GetKeyUp(flyHome))
        {
            drone.flyHome();
        }

        if (Input.GetKeyUp(uploadTestMission))
        {
            Vector3 drone_pos = drone.gameObjectPointer.transform.localPosition;
            Vector3 test_waypoint_1 = new Vector3(drone_pos.x + 3.0f, drone_pos.y + 4.0f, drone_pos.z);
            Vector3 test_waypoint_2 = new Vector3(drone_pos.x + 4.0f, drone_pos.y + 4.0f, drone_pos.z + 2.0f);
            Vector3 test_waypoint_3 = new Vector3(drone_pos.x + 1.0f, drone_pos.y + 4.0f, drone_pos.z + 4.0f);

            drone.AddWaypoint(test_waypoint_1);
            drone.AddWaypoint(test_waypoint_2);
            drone.AddWaypoint(test_waypoint_3);

            drone.uploadMission();
        }

        if (Input.GetKeyUp(uploadUserMission))
        {
            drone.uploadMission();
        }

        if (Input.GetKeyDown(runMission))
        {
            drone.startMission();
        }



    }
}
