using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;


public interface ROSDroneConnectionInterface
{
    // Initlization function
    void InitilizeDrone(int uniqueID, string droneIP, int dronePort, List<string> droneSubscribers, bool simFlight);

    // Query state variables for Informative UI and misc. info
    bool HasAuthority();
    Quaternion GetAttitude();
    NavSatFixMsg GetGPSPosition();
    float GetGPSHealth();
    Vector3 GetVelocity();
    float GetHeightAboveTakeoff();
    Vector3 GetGimbleJointAngles();
    double GetHomeLat();
    double GetHomeLong();

    // Control drone
    void StartMission();
    void PauseMission();
    void ResumeMission();
    void UpdateMission();
    void LandDrone();
    void FlyHome();

    // Close ROS Connection
    void DisconnectROSConnection();
    // Optional in the future
    // void DoTask()

}

public interface ROSSensorConnectionInterface
{
    // Initlization function
    void InitilizeSensor(int uniqueID, string sensorIP, int sensorPort, List<string> sensorSubscribers);

    // Close ROS Connection
    void DisconnectROSConnection();

    // Anything else common across sensors?

}