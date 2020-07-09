using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using UnityEditor;
using System.IO;
using ISAACS;

public class ROSManager : MonoBehaviour {

    // Let's gooooo

    /// <summary>
    /// Drone Types supported by ISAACS System
    /// </summary>
    public enum DroneType { M100, M210, M600, Sprite };

    /// <summary>
    /// Drone Subscribers supported by ISAACS System
    /// </summary>
    public enum DroneSubscribers { attitude, battery_state, flight_status, gimbal_angle, gps_health, gps_position, rtk_position, imu, rc, velocity, height_above_takeoff, local_position };

    /// <summary>
    /// Sensor types supported by ISAACS System
    /// </summary>
    public enum SensorType { PointCloud, Mesh, LAMP, PCFace };

    /// <summary>
    /// Sensor subscribers supported by ISAACS System
    /// </summary>
    public enum SensorSubscribers { surface_pointcloud, mesh,
    colorized_points_0, colorized_points_1, colorized_points_2, colorized_points_3, colorized_points_4, colorized_points_5,
        colorized_points_faced_0, colorized_points_faced_1, colorized_points_faced_2, colorized_points_faced_3, colorized_points_faced_4, colorized_points_faced_5
    };

    /// <summary>
    /// All information required to be set by the user in the Editor to create a drone connection
    /// </summary>
    [System.Serializable]
    public class ROSDroneConnectionInput
    {
        public string droneName;
        public string ipAddress; // is this int32?
        public int port;
        public DroneType droneType;
        public List<DroneSubscribers> droneSubscribers;
        public bool simFlight;
        public List<ROSSensorConnectionInput> attachedSensors;
    }

    /// <summary>
    /// All information required to be set by the user in the Editor to create a sensor connection
    /// </summary>
    [System.Serializable]
    public class ROSSensorConnectionInput
    {
        public string sensorName;
        public string ipAddress;
        public int port;
        public SensorType sensorType;
        public List<SensorSubscribers> sensorSubscribers;
    }

    public List<ROSDroneConnectionInput> DronesList;
    //public List<ROSSensorConnectionInput> SensorsList;

    private Dictionary<int, ROSDroneConnectionInterface> ROSDroneConnections = new Dictionary<int, ROSDroneConnectionInterface>();
    private Dictionary<int, ROSSensorConnectionInterface> ROSSensorConnections = new Dictionary<int, ROSSensorConnectionInterface>();

    public bool success = false;
    public int uniqueID = 0;

    /// <summary>
    /// Initlizie all drones and sensors
    /// </summary>
    void Start ()
    {
        foreach ( ROSDroneConnectionInput rosDroneConnectionInput in DronesList)
        {
            InstantiateDrone(rosDroneConnectionInput);
        }
    }

    /// <summary>
    /// Create a Drone gameobject and attach DroneFlightSim, required ROSDroneConnnection and initilize the ROS connection.
    /// </summary>
    /// <param name="rosDroneConnectionInput"></param>
    private void InstantiateDrone(ROSDroneConnectionInput rosDroneConnectionInput)
    {
        // All the variables required to create the drone
        DroneType droneType = rosDroneConnectionInput.droneType;
        string droneIP = rosDroneConnectionInput.ipAddress;
        int dronePort = rosDroneConnectionInput.port;
        bool simFlight = rosDroneConnectionInput.simFlight;
        List<string> droneSubscribers = new List<string>();

        foreach (DroneSubscribers subscriber in rosDroneConnectionInput.droneSubscribers)
        {
            droneSubscribers.Add(subscriber.ToString());
        }

        // Create a new drone
        Drone droneInstance = new Drone(WorldProperties.worldObject.transform.position, uniqueID);
        GameObject droneGameObject = droneInstance.gameObjectPointer;
        droneGameObject.name = rosDroneConnectionInput.droneName;

        // Add drone sim manager script on the drone
        DroneSimulationManager droneSim = droneGameObject.AddComponent<DroneSimulationManager>();
        droneGameObject.GetComponent<DroneProperties>().droneSimulationManager = droneSim;
        droneSim.InitDroneSim(droneInstance);

        // Add corresponding ros drone connection script
        switch (droneType)
        {
            case DroneType.M100:
                Debug.Log("M100 created");
                M100_ROSDroneConnection M100_rosDroneConnection = droneGameObject.AddComponent<M100_ROSDroneConnection>();
                M100_rosDroneConnection.InitilizeDrone(uniqueID, droneIP, dronePort, droneSubscribers, simFlight);
                droneGameObject.GetComponent<DroneProperties>().droneROSConnection = M100_rosDroneConnection;
                ROSDroneConnections.Add(uniqueID,M100_rosDroneConnection);
                break;

            case DroneType.M210:
                Debug.Log("M210 created");
                M210_ROSDroneConnection M210_rosDroneConnection = droneGameObject.AddComponent<M210_ROSDroneConnection>();
                M210_rosDroneConnection.InitilizeDrone(uniqueID, droneIP, dronePort, droneSubscribers, simFlight);
                droneGameObject.GetComponent<DroneProperties>().droneROSConnection = M210_rosDroneConnection;
                ROSDroneConnections.Add(uniqueID, M210_rosDroneConnection);
                break;

            case DroneType.M600:
                Debug.Log("M600 created");
                M600_ROSDroneConnection M600_rosDroneConnection = droneGameObject.AddComponent<M600_ROSDroneConnection>();
                M600_rosDroneConnection.InitilizeDrone(uniqueID, droneIP, dronePort, droneSubscribers, simFlight);
                droneGameObject.GetComponent<DroneProperties>().droneROSConnection = M600_rosDroneConnection;
                ROSDroneConnections.Add(uniqueID, M600_rosDroneConnection);
                break;

            case DroneType.Sprite:
                Debug.Log("Sprite class not implemented created");
                //Sprite_ROSDroneConnection drone_rosDroneConnection = drone.AddComponent<Sprite_ROSDroneConnection>();
                break;
            
            default:
                Debug.Log("No drone type selected");
                return;
        }

        // Create attached sensors
        // @Jasmine: We should refine this based on what makes the most sense 
        foreach (ROSSensorConnectionInput rosSensorInput in rosDroneConnectionInput.attachedSensors)
        {
            // @Jasmine: We might need a sensor properties type script to connect a sensor back to the drone?
            ROSSensorConnectionInterface sensor = InstantiateSensor(rosSensorInput);
            droneInstance.AddSensor(sensor);
        }

        uniqueID ++;
    }

    /// <summary>
    /// Create a Sensor gameobject and attach & init required ROSSensorConnnection.
    /// </summary>
    /// <param name="rosSensorConnectionInput"></param>
    private ROSSensorConnectionInterface InstantiateSensor(ROSSensorConnectionInput rosSensorConnectionInput)
    {
        SensorType sensorType = rosSensorConnectionInput.sensorType;
        string sensorIP = rosSensorConnectionInput.ipAddress;
        int sensorPort = rosSensorConnectionInput.port;
        List<string> sensorSubscribers = new List<string>();
        ROSSensorConnectionInterface rosSensorConnection = null;

        foreach (SensorSubscribers subscriber in rosSensorConnectionInput.sensorSubscribers)
        {
            sensorSubscribers.Add(subscriber.ToString());
        }

        GameObject sensor = new GameObject(rosSensorConnectionInput.sensorName);
        sensor.transform.parent = this.transform;

        switch (sensorType)
        {
            case SensorType.PointCloud:
                Debug.Log("PointCloud Sensor created");
                PointCloudSensor_ROSSensorConnection pcSensor_rosSensorConnection = sensor.AddComponent<PointCloudSensor_ROSSensorConnection>();
                pcSensor_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, pcSensor_rosSensorConnection);
                rosSensorConnection = pcSensor_rosSensorConnection;
                break;

            case SensorType.Mesh:
                Debug.Log("Mesh Sensor created");
                MeshSensor_ROSSensorConnection meshSensor_rosSensorConnection = sensor.AddComponent<MeshSensor_ROSSensorConnection>();
                meshSensor_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, meshSensor_rosSensorConnection);
                rosSensorConnection = meshSensor_rosSensorConnection;
                break;

            case SensorType.LAMP:
                Debug.Log("LAMP Sensor created");
                LampSensor_ROSSensorConnection lamp_rosSensorConnection = sensor.AddComponent<LampSensor_ROSSensorConnection>();
                lamp_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, lamp_rosSensorConnection);
                rosSensorConnection = lamp_rosSensorConnection;
                break;

            case SensorType.PCFace:
                Debug.Log("PCFace Sensor created");
                PCFaceSensor_ROSSensorConnection pcFace_rosSensorConnection = sensor.AddComponent<PCFaceSensor_ROSSensorConnection>();
                pcFace_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, pcFace_rosSensorConnection);
                rosSensorConnection = pcFace_rosSensorConnection;
                break;

            default:
                Debug.Log("No sensor type selected");
                return null;
        }


        // Add sensor to list of sensors in World Properties
        WorldProperties.AddSensor(uniqueID, sensor);

        uniqueID++;

        return rosSensorConnection;

    }
    
    void OnApplicationQuit()
    {
        foreach (ROSDroneConnectionInterface rosDroneConnection in ROSDroneConnections.Values)
        {
            rosDroneConnection.DisconnectROSConnection();
        }

        foreach (ROSSensorConnectionInterface rosSensorConnection in ROSSensorConnections.Values)
        {
            rosSensorConnection.DisconnectROSConnection();
        }
    }

}


