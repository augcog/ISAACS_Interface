using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using UnityEditor;
using System.IO;
using ISAACS;

public class ROSManager : MonoBehaviour
{

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
    public enum SensorType { PointCloud, Mesh, LAMP, PCFace, Image, Zed};

    /// <summary>
    /// Sensor subscribers supported by ISAACS System
    /// </summary>
    public enum SensorSubscribers
    {
        surface_pointcloud, mesh, mesh2,
        colorized_points_0, colorized_points_1, colorized_points_2, colorized_points_3, colorized_points_4, colorized_points_5,
        colorized_points_faced_0, colorized_points_faced_1, colorized_points_faced_2, colorized_points_faced_3, colorized_points_faced_4, colorized_points_faced_5, fpv_camera_images
    };

    /// <summary>
    /// All information required to be set by the user in the Editor to create a drone connection
    /// </summary>
    [System.Serializable]
    public class ROSDroneConnectionInput
    {
        public string droneName;
        public string url;
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
        public string url;
        public int port;
        public SensorType sensorType;
        public List<SensorSubscribers> sensorSubscribers;
        //public List<VideoPlayers> videoPlayers;
        public VideoPlayers videoPlayers;
    }

    [System.Serializable]
    public enum VideoPlayers { None, LeftVideo, RightVideo, BackVideo, FrontVideo };

    public List<ROSDroneConnectionInput> DronesList;
    //public List<ROSSensorConnectionInput> SensorsList;

    private Dictionary<int, ROSDroneConnectionInterface> ROSDroneConnections = new Dictionary<int, ROSDroneConnectionInterface>();
    private Dictionary<int, ROSSensorConnectionInterface> ROSSensorConnections = new Dictionary<int, ROSSensorConnectionInterface>();

    public bool success = false;
    public int uniqueID = 0;

    /// <summary>
    /// Initialize all drones and sensors
    /// </summary>
    void Start()
    {
        foreach (ROSDroneConnectionInput rosDroneConnectionInput in DronesList)
        {
            InstantiateDrone(rosDroneConnectionInput);
        }

        success = true;
        WorldProperties.SelectNextDrone();
    }

    /// <summary>
    /// Create a Drone gameobject and attach DroneFlightSim, required ROSDroneConnnection and initialize the ROS connection.
    /// </summary>
    /// <param name="rosDroneConnectionInput"></param>
    private void InstantiateDrone(ROSDroneConnectionInput rosDroneConnectionInput)
    {
        // All the variables required to create the drone
        DroneType droneType = rosDroneConnectionInput.droneType;
        string droneIP = rosDroneConnectionInput.url;
        int dronePort = rosDroneConnectionInput.port;
        bool simFlight = rosDroneConnectionInput.simFlight;
        List<string> droneSubscribers = new List<string>();

        foreach (DroneSubscribers subscriber in rosDroneConnectionInput.droneSubscribers)
        {
            droneSubscribers.Add(subscriber.ToString());
        }

        // Create a new drone
        Drone droneInstance = new Drone(WorldProperties.worldObject.transform.position, uniqueID);
        Debug.Log("Drone that was just made " + droneInstance.gameObjectPointer.name);
        DroneProperties droneProperties = droneInstance.droneProperties;
        GameObject droneGameObject = droneInstance.gameObjectPointer;
        droneGameObject.name = rosDroneConnectionInput.droneName;

        ROSDroneConnectionInterface rosDroneConnection = null;

        // Add corresponding ros drone connection script
        switch (droneType)
        {
            case DroneType.M100:
                Debug.Log("M100 created");
                M100_ROSDroneConnection M100_rosDroneConnection = droneGameObject.AddComponent<M100_ROSDroneConnection>();
                M100_rosDroneConnection.InitilizeDrone(uniqueID, droneIP, dronePort, droneSubscribers, simFlight, droneProperties);
                rosDroneConnection = M100_rosDroneConnection;
                droneGameObject.GetComponent<DroneProperties>().droneROSConnection = M100_rosDroneConnection;
                ROSDroneConnections.Add(uniqueID, M100_rosDroneConnection);
                break;

            case DroneType.M210:
                Debug.Log("M210 created");
                M210_ROSDroneConnection M210_rosDroneConnection = droneGameObject.AddComponent<M210_ROSDroneConnection>();
                M210_rosDroneConnection.InitilizeDrone(uniqueID, droneIP, dronePort, droneSubscribers, simFlight, droneProperties);
                rosDroneConnection = M210_rosDroneConnection;
                droneGameObject.GetComponent<DroneProperties>().droneROSConnection = M210_rosDroneConnection;
                ROSDroneConnections.Add(uniqueID, M210_rosDroneConnection);
                break;

            case DroneType.M600:
                Debug.Log("M600 created");
                M600_ROSDroneConnection M600_rosDroneConnection = droneGameObject.AddComponent<M600_ROSDroneConnection>();
                M600_rosDroneConnection.InitilizeDrone(uniqueID, droneIP, dronePort, droneSubscribers, simFlight, droneProperties);
                rosDroneConnection = M600_rosDroneConnection;
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
        foreach (ROSSensorConnectionInput rosSensorInput in rosDroneConnectionInput.attachedSensors)
        {
            ROSSensorConnectionInterface sensor = InstantiateSensor(rosSensorInput);
            droneInstance.AddSensor(sensor);
        }

        // Initilize drone sim manager script on the drone
        DroneSimulationManager droneSim = droneGameObject.GetComponent<DroneSimulationManager>();
        droneSim.InitDroneSim();
        droneProperties.droneSimulationManager = droneSim;
 
        // Get DroneMenu and instansiate.
        DroneMenu droneMenu = droneGameObject.GetComponent<DroneMenu>();
        droneMenu.InitDroneMenu(rosDroneConnection, droneSubscribers);
        droneGameObject.GetComponent<DroneProperties>().droneMenu = droneMenu;

        uniqueID++;
    }

    /// <summary>
    /// Create a Sensor gameobject and attach & init required ROSSensorConnnection.
    /// </summary>
    /// <param name="rosSensorConnectionInput"></param>
    private ROSSensorConnectionInterface InstantiateSensor(ROSSensorConnectionInput rosSensorConnectionInput)
    {
        SensorType sensorType = rosSensorConnectionInput.sensorType;
        string sensorIP = rosSensorConnectionInput.url;
        int sensorPort = rosSensorConnectionInput.port;
        List<string> sensorSubscribers = new List<string>();
        ROSSensorConnectionInterface rosSensorConnection = null;

        foreach (SensorSubscribers subscriber in rosSensorConnectionInput.sensorSubscribers)
        {
            sensorSubscribers.Add(subscriber.ToString());
        }

        GameObject sensor = new GameObject(rosSensorConnectionInput.sensorName);
        sensor.transform.parent = this.transform;
        //sensor.transform.localPosition = new Vector3(7.33f, 3.387f, 15.27f);
        //sensor.transform.localRotation = Quaternion.Euler(new Vector3(-5.811f,-208.85f,1.375f));
        //sensor.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

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

            // We don't use this sensor type anymore, as it's point-cloud based rather than PCFace-based.
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

            case SensorType.Image:
                Debug.Log("Camera Stream created");
                CameraSensor_ROSSensorConnection camera_rosSensorConnection = sensor.AddComponent<CameraSensor_ROSSensorConnection>();
                VideoPlayers videoType = rosSensorConnectionInput.videoPlayers;
                switch (videoType)
                {
                    case VideoPlayers.BackVideo:
                        camera_rosSensorConnection.SetVideoType("BackVideo");
                        break;
                    case VideoPlayers.FrontVideo:
                        camera_rosSensorConnection.SetVideoType("FrontVideo");
                        break;
                    case VideoPlayers.LeftVideo:
                        camera_rosSensorConnection.SetVideoType("LeftVideo");
                        break;
                    case VideoPlayers.RightVideo:
                        camera_rosSensorConnection.SetVideoType("RightVideo");
                        break;
                    default:
                        Debug.Log("No Video Type Selected.");
                        return null;
                }
                camera_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, camera_rosSensorConnection);
                rosSensorConnection = camera_rosSensorConnection;
                break;

            case SensorType.Zed:
                Debug.Log("Zed Sensor created");
                sensorSubscribers.Clear();
                sensorSubscribers.Add("mesh2");
                MeshSensor_ROSSensorConnection zedSensor_rosSensorConnection = sensor.AddComponent<MeshSensor_ROSSensorConnection>();
                zedSensor_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, zedSensor_rosSensorConnection);
                rosSensorConnection = zedSensor_rosSensorConnection;

                Shader shader = Shader.Find("Custom/ZedMesh");
                zedSensor_rosSensorConnection.UpdateVisualizer(shader);
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



