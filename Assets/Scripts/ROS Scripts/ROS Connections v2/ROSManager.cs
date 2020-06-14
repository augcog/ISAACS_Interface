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

    public enum DroneType { M100, M210, M600, Sprite };
    public enum DroneSubscribers { attitude, battery_state, flight_status, gimbal_angle, gps_health, gps_position, imu, rc, velocity, height_above_takeoff, local_position };

    public enum SensorType { PointCloud, Mesh, LAMP, PCFace };
    public enum SensorSubscribers { surface_pointcloud, mesh,
        colorized_points_0, colorized_points_1, colorized_points_2, colorized_points_3, colorized_points_4, colorized_points_5,
        colorized_points_faced_0, colorized_points_faced_1, colorized_points_faced_2, colorized_points_faced_3, colorized_points_faced_4, colorized_points_faced_5
    };

    [System.Serializable]
    public class ROSDroneConnectionInput
    {
        public string droneName;
        public string droneTag;
        public string ipAddress; // is this int32?
        public int port;
        public DroneType droneType;
        public List<DroneSubscribers> droneSubscribers;
        public bool simFlight;
    }

    [System.Serializable]
    public class ROSSensorConnectionInput
    {
        public string sensorName;
        public string sensorTag;
        public string ipAddress;
        public int port;
        public SensorType sensorType;
        public List<SensorSubscribers> sensorSubscribers;
    }

    public List<ROSDroneConnectionInput> DronesList;
    public List<ROSSensorConnectionInput> SensorsList;

    private Dictionary<int, ROSDroneConnectionInterface> ROSDroneConnections = new Dictionary<int, ROSDroneConnectionInterface>();
    private Dictionary<int, ROSSensorConnectionInterface> ROSSensorConnections = new Dictionary<int, ROSSensorConnectionInterface>();

    public bool success = false;
    public int uniqueID = 0;

    // Use this for initialization
    void Start ()
    {
        foreach ( ROSDroneConnectionInput rosDroneConnectionInput in DronesList)
        {
            InstantiateDrone(rosDroneConnectionInput);
        }

        foreach (ROSSensorConnectionInput rosSensorConnectionInput in SensorsList)
        {
            InstantiateSensor(rosSensorConnectionInput);
        }
    }

    private void InstantiateDrone(ROSDroneConnectionInput rosDroneConnectionInput)
    {
        DroneType droneType = rosDroneConnectionInput.droneType;
        string droneIP = rosDroneConnectionInput.ipAddress;
        int dronePort = rosDroneConnectionInput.port;
        bool simFlight = rosDroneConnectionInput.simFlight;
        List<string> droneSubscribers = new List<string>();

        foreach (DroneSubscribers subscriber in rosDroneConnectionInput.droneSubscribers)
        {
            droneSubscribers.Add(subscriber.ToString());
        }

        //GameObject drone = new GameObject(rosDroneConnectionInput.droneName);
        //drone.transform.parent = this.transform;

        Drone droneInstance = new Drone(WorldProperties.worldObject.transform.position);
        GameObject droneGameObject = droneInstance.gameObjectPointer;
        droneGameObject.tag = rosDroneConnectionInput.droneTag;
        droneGameObject.name = rosDroneConnectionInput.droneName;

        // Add DroneFlightSim
        // Peru 6:10:20
        DroneSimulationManager droneSim = droneGameObject.AddComponent<DroneSimulationManager>();
        droneGameObject.GetComponent<DroneProperties>().droneSimulationManager = droneSim;
        droneSim.InitDroneSim(droneInstance);

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

        // TODO: Uncomment after implementing ROSDroneConnection
        // drone.InitilizeDrone(uniqueID, droneIP, dronePort, droneSubscribers)

        uniqueID ++;
    }

    private void InstantiateSensor(ROSSensorConnectionInput rosSensorConnectionInput)
    {
        SensorType sensorType = rosSensorConnectionInput.sensorType;
        string sensorIP = rosSensorConnectionInput.ipAddress;
        int sensorPort = rosSensorConnectionInput.port;
        List<string> sensorSubscribers = new List<string>();

        foreach (SensorSubscribers subscriber in rosSensorConnectionInput.sensorSubscribers)
        {
            sensorSubscribers.Add(subscriber.ToString());
        }

        GameObject sensor = new GameObject(rosSensorConnectionInput.sensorName);
        sensor.transform.parent = this.transform;
        sensor.tag = rosSensorConnectionInput.sensorTag;

        switch (sensorType)
        {
            case SensorType.PointCloud:
                Debug.Log("PointCloud Sensor created");
                PointCloudSensor_ROSSensorConnection pcSensor_rosSensorConnection = sensor.AddComponent<PointCloudSensor_ROSSensorConnection>();
                pcSensor_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, pcSensor_rosSensorConnection);
                break;

            case SensorType.Mesh:
                Debug.Log("Mesh Sensor created");
                MeshSensor_ROSSensorConnection meshSensor_rosSensorConnection = sensor.AddComponent<MeshSensor_ROSSensorConnection>();
                meshSensor_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, meshSensor_rosSensorConnection);
                break;

            case SensorType.LAMP:
                Debug.Log("LAMP Sensor created");
                LampSensor_ROSSensorConnection lamp_rosSensorConnection = sensor.AddComponent<LampSensor_ROSSensorConnection>();
                lamp_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, lamp_rosSensorConnection);
                break;

            case SensorType.PCFace:
                Debug.Log("PCFace Sensor created");
                PCFaceSensor_ROSSensorConnection pcFace_rosSensorConnection = sensor.AddComponent<PCFaceSensor_ROSSensorConnection>();
                pcFace_rosSensorConnection.InitilizeSensor(uniqueID, sensorIP, sensorPort, sensorSubscribers);
                ROSSensorConnections.Add(uniqueID, pcFace_rosSensorConnection);
                break;

            default:
                Debug.Log("No sensor type selected");
                return;
        }

        // TODO: Uncomment after implementing ROSDroneConnection
        // sensor.InitilizeSensor(uniqueID, sensorIP, sensorPort ,sensorSubscribers)
        uniqueID++;
    }

    // Update is called once per frame
    void Update () {
		
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


