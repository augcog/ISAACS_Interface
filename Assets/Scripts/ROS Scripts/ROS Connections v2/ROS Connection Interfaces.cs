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
    void InitilizeDrone(int uniqueID, string droneIP, int dronePort, List<string> droneSubscribers, bool simFlight, DroneProperties droneProp);

    // Query state variables for Informative UI and misc. info

    /// Get the value of a certain topic.
    /// To be used by the UI for further abstraction
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    string GetValueByTopic(string topic);

    /// <summary>
    /// State of Unity interface authority over controlling the drone
    /// </summary>
    /// <returns></returns>
    bool HasAuthority();

    /// <summary>
    /// Drone attitude
    /// </summary>
    /// <returns></returns>
    Quaternion GetAttitude();

    /// <summary>
    /// Drone GPS position
    /// </summary>
    /// <returns></returns>
    NavSatFixMsg GetGPSPosition();

    /// <summary>
    /// Drone GPS Health
    /// </summary>
    /// <returns></returns>
    float GetGPSHealth();

    /// <summary>
    /// Drone velocity
    /// </summary>
    /// <returns></returns>
    Vector3 GetVelocity();

    /// <summary>
    /// Drone height above takeoff height
    /// </summary>
    /// <returns></returns>
    float GetHeightAboveTakeoff();

    /// <summary>
    /// Gimbal angles (if available)
    /// </summary>
    /// <returns></returns>
    Vector3 GetGimbalJointAngles();

    /// <summary>
    /// Drone home latitude
    /// </summary>
    /// <returns></returns>
    double GetHomeLat();

    /// <summary>
    /// Drone home longitude
    /// </summary>
    /// <returns></returns>
    double GetHomeLong();

    /// Drone control methods 

    /// <summary>
    /// Infor the ROS Connection that the uploaded waypoint mission has been completed.
    /// </summary>
    void UploadedMissionCompleted();

    /// <summary>
    /// Start the drone mission
    /// </summary>
    void StartMission();

    /// <summary>
    /// Pause drone mission
    /// </summary>
    void PauseMission();

    /// <summary>
    /// Resume drone mission
    /// </summary>
    void ResumeMission();

    /// <summary>
    /// Update drone mission
    /// </summary>
    void UpdateMission();

    /// <summary>
    /// Land drone
    /// </summary>
    void LandDrone();

    /// <summary>
    /// Sent drone to home position
    /// </summary>
    void FlyHome();

    /// <summary>
    /// Disconnect the ROS connection
    /// </summary>
    void DisconnectROSConnection();

    // Optional in the future
    // void DoTask()

}

public interface ROSSensorConnectionInterface
{
    // Initlization function
    void InitilizeSensor(int uniqueID, string sensorIP, int sensorPort, List<string> sensorSubscribers);

    /// <summary>
    /// Disconnect the ROS connection
    /// </summary>    
    void DisconnectROSConnection();

    /// <summary>
    /// Get the name of the sensor
    /// </summary>
    /// <returns></returns>
    string GetSensorName();

    /// <summary>
    /// Returns a dictionary of connected subscriber topics (which are unique identifiers) as keys and T/F as value representing the status of subscriber
    /// </summary>
    /// <returns></returns>
    Dictionary<string,bool> GetSensorSubscribers();

    /// <summary>
    /// Function to disconnect a specific subscriber
    /// </summary>
    /// <param name="subscriberID"></param>
    void Unsubscribe(string subscriberTopic);

    /// <summary>
    /// Function to connect a specific subscriber
    /// </summary>
    /// <param name="subscriberID"></param>
    void Subscribe(string subscriberTopic);

    /// <summary>
    /// Sets the local orientation of the sensor.
    /// </summary>
    /// <param name="quaternion">Orientation to apply the the sensor, usually the quaternion from ENU to FLU.</param>
    void SetLocalOrientation(Quaternion quaternion);

    /// <summary>
    /// Sets the local position of the sensor.
    /// </summary>
    /// <param name="position">Position of the sensor. Usually the position of its drone in Unity.</param>
    void SetLocalPosition(Vector3 position);

    /// <summary>
    /// Sets the local scale of the sensor.
    /// </summary>
    /// <param name="scale">Scale of the sensor. Usually 1.</param>
    void SetLocalScale(Vector3 scale);
  
    // Anything else common across sensors?

}