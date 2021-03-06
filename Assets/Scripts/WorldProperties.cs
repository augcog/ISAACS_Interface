namespace ISAACS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
        private static Drone selectedDrone;
        private static Queue<Drone> dronesQueue;

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

        // Enum for radiation data types supported
        public enum RadiationDataType { gamma, neutron };
        [Header("Hardcoded logic for neuton/gamma rays")]
        public static RadiationDataType radiationDataType = RadiationDataType.gamma;

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

            dronesQueue = new Queue<Drone>();

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
            /*
            // close wall & road & field intersection.
            GameObject meshScale1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshScale1.name = "meshScale2";
            meshScale1.transform.parent = this.transform;
            meshScale1.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915317, -122.337779, 5.226));
            meshScale1.transform.localScale = Vector3.one * (0.5f);

            GameObject aircraft2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aircraft2.name = "aircraft 2";
            aircraft2.transform.parent = this.transform;
            aircraft2.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915070, -122.337683, 5.488));
            aircraft2.transform.localScale = Vector3.one * (0.05f);

            // Tree branch parkinglot corner.
            GameObject meshScale2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshScale2.name = "meshScale1";
            meshScale2.transform.parent = this.transform;
            meshScale2.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915008, -122.337913, 4.338));
            meshScale2.transform.localScale = Vector3.one * (0.5f);

            GameObject aircraft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aircraft.name = "aircraft 1";
            aircraft.transform.parent = this.transform;
            aircraft.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915028, -122.337692, 4.668));
            aircraft.transform.localScale = Vector3.one * (0.05f);
            // Far wall & road & field instersection.
            GameObject meshScale3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshScale3.name = "meshScale3";
            meshScale3.transform.parent = this.transform;
            meshScale3.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915683, -122.337788, 4.022));
            meshScale3.transform.localScale = Vector3.one * (0.5f);

            GameObject aircraft3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aircraft3.name = "aircraft 3";
            aircraft3.transform.parent = this.transform;
            aircraft3.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915051, -122.337695, 3.997));
            aircraft3.transform.localScale = Vector3.one * (0.05f);
            // Far tree
            GameObject meshScale4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshScale4.name = "meshScale4";
            meshScale4.transform.parent = this.transform;
            meshScale4.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915880, -122.338171, 6.902));
            meshScale4.transform.localScale = Vector3.one * (0.5f);

            // Stump near the tree closest to the house.
            GameObject meshScale5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshScale5.name = "meshScale5";
            meshScale5.transform.parent = this.transform;
            meshScale5.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915376, -122.338201, 1.459));
            meshScale5.transform.localScale = Vector3.one * (0.5f);

            GameObject aircraft5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aircraft5.name = "aircraft 5";
            aircraft5.transform.parent = this.transform;
            aircraft5.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915028, -122.337711, 1.858));
            aircraft5.transform.localScale = Vector3.one * (0.05f);

            // Power pole near the fork in the road.
            GameObject meshScale7 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshScale7.name = "meshScale7";
            meshScale7.transform.parent = this.transform;
            meshScale7.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.914904, -122.337333, 2.237));
            meshScale7.transform.localScale = Vector3.one * (0.5f);

            GameObject aircraft7 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aircraft7.name = "aircraft 7";
            aircraft7.transform.parent = this.transform;
            aircraft7.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915058, -122.337716, 2.631));
            aircraft7.transform.localScale = Vector3.one * (0.05f);

            // Stump at the entrance to the parking lot.
            GameObject meshScale6 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshScale6.name = "meshScale6";
            meshScale6.transform.parent = this.transform;
            meshScale6.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915007, -122.337491, 2.522));
            meshScale6.transform.localScale = Vector3.one * (0.5f);

            GameObject aircraft6 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aircraft6.name = "aircraft 6";
            aircraft6.transform.parent = this.transform;
            aircraft6.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915048, -122.337703, 2.948));
            aircraft6.transform.localScale = Vector3.one * (0.05f);
            */
            //AddClipShader(this.transform);
            
        }

        /// <summary>
        /// Cycle through the connected drones
        /// </summary>
        public static Drone SelectNextDrone()
        {
            if(selectedDrone != null)
            {
                Debug.Log("Enquing selected drone");
                selectedDrone.gameObjectPointer.GetComponent<DroneProperties>().DeselectDrone();
                dronesQueue.Enqueue(selectedDrone);
            }

            if (dronesQueue.Count > 0)
            {
                Drone nextDrone = dronesQueue.Dequeue();
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
        public static Drone GetSelectedDrone()
        {
            return selectedDrone;
        }

        /// <summary>
        /// Add a drone to the drones dictionary
        /// </summary>
        /// <param name="drone"></param>
        public static void AddDrone(Drone drone)
        {
            Debug.Log("Added drone " + drone);
            dronesQueue.Enqueue(drone);
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
