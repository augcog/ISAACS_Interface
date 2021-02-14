﻿namespace ISAACS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using ROSBridgeLib;
    using ROSBridgeLib.std_msgs;
    using ROSBridgeLib.interface_msgs;
    using Mapbox.Unity.Map;
    using Mapbox.Utils;
    using ROSBridgeLib.sensor_msgs;
    using JetBrains.Annotations;

    // TODO: we might want to lock FPS!
    /// <summary>
    /// This is the only class that should have static variables or functions that are consistent throughout the entire program.
    /// </summary>
    public class WorldProperties : MonoBehaviour
    {
        [Header("Required Prefabs")]
        public GameObject droneBaseObject;
        public GameObject waypointBaseObject;
        public SensorManager sensorManagerBaseObject;

        [Header("Drone variables")]
        private static Drone_v2 selectedDrone;
        private static Dictionary<int, Drone_v2> droneDict;
        private static Queue<Drone_v2> dronesQueue;

        [Header("Sensor variables")]
        public static GameObject selectedSensor;
        public static Dictionary<int, GameObject> sensorDict;
        public static SensorManager sensorManager;

        [Header("Misc. State variables")]
        public static GameObject worldObject;
        public static GameObject placementPlane;
        public static bool DJI_SIM = false;

        public static Vector3 actualScale;
        public static Vector3 currentScale;

        private static Shader clipShader;

        [Header("Google Earth Mesh")]
        // ROS-Unity conversion variables
        public double MeshLatitude;
        public double MeshLongitude;
        // Relative to the surface of the WGS 84 Ellipsoid
        public double MeshAltitude;
        public Vector3 MeshRotation;
        public Vector3 MeshScale;

        public GameObject MeshEarthPrefab;

        [Header("Mission Center Coordinates")]
        // ROS-Unity conversion variables
        // MCLat/Long should be set in editor to location of flight. 
        // Correct conversion is dependent on where we wish to convert GPS <--> Unity coords
        public double MCLatitude;
        public double MCLongitude;
        // Relative to the surface of the WGS 84 Ellipsoid
        public double MCAltitude;

        private static double Lat0;
        private static double Lng0;
        private static double Alt0;
        /// <summary>
        /// Cosine of missionCenterLat. This correction is applied to the longitude calculations.
        /// </summary>
        private static double lngCorrection;
        /// <summary>
        /// Radius of the Earth in meters.
        /// </summary>
        private const double EARTH_RADIUS = 6378137;
        private const double TO_RADIANS = Math.PI / 180.0;
        private const double FROM_RADIANS = 180.0 / Math.PI;

        // Use this for initialization
        void Start()
        {
            selectedDrone = null;

            dronesQueue = new Queue<Drone_v2>();
            droneDict = new Dictionary<int, Drone_v2>();

            selectedSensor = null;
            sensorDict = new Dictionary<int, GameObject>();

            worldObject = gameObject;
            sensorManager = sensorManagerBaseObject;

            placementPlane = GameObject.FindWithTag("Ground");

            actualScale = new Vector3(1, 1, 1);
            currentScale = new Vector3(1, 1, 1);

            Lat0 = MCLatitude;
            Lng0 = MCLongitude;
            Alt0 = MCAltitude;
            lngCorrection = Math.Cos(MCLatitude * TO_RADIANS);
            clipShader = GameObject.FindWithTag("Ground").GetComponent<Renderer>().material.shader;

            MeshEarthPrefab.transform.localPosition = GPSCoordToUnityCoord(new GPSCoordinate(MeshLatitude, MeshLongitude, MeshAltitude));
            MeshEarthPrefab.transform.localRotation = Quaternion.Euler(MeshRotation);
            MeshEarthPrefab.transform.localScale = MeshScale;
           
            //AddClipShader(this.transform);
            
        }

        /// <summary>
        /// Cycle through the connected drones
        /// </summary>
        public static Drone_v2 SelectNextDrone()
        {
            //Debug.Log("Dictionary right now: " + droneDict.Count);


            if (selectedDrone != null)
            {
                Debug.Log("Enquing selected drone");
                selectedDrone.gameObjectPointer.GetComponent<DroneProperties>().DeselectDrone();
                dronesQueue.Enqueue(selectedDrone);
            }

            if (dronesQueue.Count > 0)
            {
                Drone_v2 nextDrone = dronesQueue.Dequeue();
                nextDrone.droneProperties.SelectDrone();


                if (worldObject.GetComponent<M210_Flight_TestManager>() != null)
                {
                    M210_Flight_TestManager flight_TestManager = worldObject.GetComponent<M210_Flight_TestManager>();
                    flight_TestManager.UpdateDrone(nextDrone.droneProperties.droneROSConnection);
                }

                selectedDrone = nextDrone;

                return nextDrone;
            }
            else
            {
                Debug.Log("No drones connected");
                return null;
            }

        }
        
        /// <summary>
        /// Get the selected drone
        /// </summary>
        public static Drone_v2 GetSelectedDrone()
        {
            return selectedDrone;
        }

        /// <summary>
        /// Add a drone to the drones dictionary
        /// </summary>
        /// <param name="drone"></param>
        public static void AddDrone(Drone_v2 drone)
        {

            Debug.Log("ADDING DRONE " + drone.gameObjectPointer.name + ", ID:" + drone.id.ToString());
            dronesQueue.Enqueue(drone);
            //Debug.Log("Print Drone.ID added to dict is: " + drone.id);
            droneDict.Add(drone.id, drone);
        }

        /// <summary>
        /// Get selected sensor
        /// </summary>
        public static GameObject GetSelectedSensor()
        {
            return selectedSensor;
        }

        /// <summary>
        /// Add a sensor to the sensor dictionary
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sensor"></param>
        public static void AddSensor(int id, GameObject sensor)
        {
            sensorDict.Add(id, sensor);
        }

        /// <summary>
        /// Get list of all sensors
        /// </summary>
        public static Dictionary<int, GameObject> GetSensorDict()
        {
            return sensorDict;
        }

        /// <summary>
        /// Get list of all drones
        /// </summary>
        public static Dictionary<int, Drone_v2> GetDroneDict()
        {
            return droneDict;
        }


        /// <summary>
        /// Recursively adds the clipShader to the parent and all its children
        /// </summary>
        /// <param name="parent">The topmost container of the objects which will have the shader added to them</param>
        public static void AddClipShader(Transform parent)
    {
        if (parent.GetComponent<Renderer>())
        {
            parent.GetComponent<Renderer>().material.shader = clipShader;
        }

        foreach (Transform child in parent)
        {
            AddClipShader(child);
        }
    }

        /// <summary>
        /// Convert the given ROS NavSatFixMsg to Unity XYZ space.
        /// To Be used to convert drone coordinates to unity space
        /// </summary>
        /// <returns>Unity position vector to use within World GameObject</returns>
        public static Vector3 ROSCoordToUnityCoord(NavSatFixMsg gpsPosition)
        {
            return GPSCoordToUnityCoord(new GPSCoordinate(gpsPosition.GetLatitude(), gpsPosition.GetLongitude(), gpsPosition.GetAltitude()));
        }

        /// <summary>
        /// Convert the given GPSCoordinate to Unity XYZ space.
        /// </summary>
        /// <param name="gpsPosition"></param>
        /// <returns></returns>
        public static Vector3 GPSCoordToUnityCoord(GPSCoordinate gpsPosition)
        {
            Vector3 unityCoord = Vector3.zero;
            unityCoord.z = (float)((gpsPosition.Lat - Lat0) * TO_RADIANS * EARTH_RADIUS);
            unityCoord.x = (float)((gpsPosition.Lng - Lng0) * TO_RADIANS *  EARTH_RADIUS * lngCorrection);
            unityCoord.y = (float)(gpsPosition.Alt - Alt0);

            return unityCoord;
        }

        /// <summary>
        /// Convert Unity coordinates to ROS world lat,long,alt
        /// To be used to convert waypint unity coordinates to world lat.long.alt
        /// </summary>
        /// <param name="unityPosition"></param>
        /// <returns>GPSCoordinates</returns>
        public static GPSCoordinate UnityCoordToGPSCoord(Vector3 unityPosition)
        {
            GPSCoordinate gpsCoord = new GPSCoordinate();
            gpsCoord.x = (((double) unityPosition.x * FROM_RADIANS / (EARTH_RADIUS * lngCorrection)) + Lng0);
            gpsCoord.y = (((double) unityPosition.z * FROM_RADIANS / EARTH_RADIUS) + Lat0);
            gpsCoord.z= (((double) unityPosition.y) + Alt0);
            return gpsCoord;
        }
    }

    /// <summary>
    /// GPSCoordinates
    /// </summary>
    public struct GPSCoordinate
    {
        /// <summary>
        /// Create a GPS Coordinate
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lng">Longitude</param>
        /// <param name="alt">Altitude</param>
        public GPSCoordinate(double lat, double lng, double alt)
        {
            Lat = lat;
            Lng = lng;
            Alt = alt;
        }

        /// <summary>
        /// Latitude in Decimal Degrees.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Longitude in Decimal Degrees.
        /// </summary>
        public double Lng { get; set; }
        
        /// <summary>
        /// Altitude in meters.
        /// </summary>
        public double Alt { get; set; }
        
        /// <summary>
        /// Longitude in Decimal Degrees.
        /// </summary>
        public double x 
        { 
            get { return Lng; }
            set { Lng = value; }
        }
        
        /// <summary>
        /// Latitude in Decimal Degrees.
        /// </summary>
        public double y
        {
            get { return Lat; }
            set { Lat = value; }
        }
        
        /// <summary>
        /// Altitude in meters.
        /// </summary>
        public double z
        {
            get { return Alt; }
            set { Alt = value; }
        }
    }
}
