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

        [Header("Drone variables")]
        public static Drone selectedDrone;
        public static char nextDroneId;
        public static Dictionary<char, Drone> dronesDict;

        [Header("Sensor vairables")]
        public static GameObject selectedSensor;
        public static Dictionary<int, GameObject> sensorDict;

        [Header(" Misc. State variables")]
        public static GameObject worldObject;
        public static GameObject placementPlane;

        public static Vector3 actualScale;
        public static Vector3 currentScale;

        public static Shader clipShader;

        [Header("Mission Center Coordinates")]
        // ROS-Unity conversion variables
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

        //Unity to lat --> multiply by scale; lat to Unity --> divide by scale
        public static float Unity_X_To_Lat_Scale = 10.0f;
        public static float Unity_Y_To_Alt_Scale = 10.0f;
        public static float Unity_Z_To_Long_Scale = 10.0f;

        // Use this for initialization
        void Start()
        {
            selectedDrone = null;
            dronesDict = new Dictionary<char, Drone>(); // Collection of all the drone classObjects

            selectedSensor = null;
            sensorDict = new Dictionary<int, GameObject>();

            nextDroneId = 'A'; // Used as an incrementing key for the dronesDict and for a piece of the communication about waypoints across the ROSBridge

            worldObject = gameObject;

            placementPlane = GameObject.FindWithTag("Ground");

            actualScale = new Vector3(1, 1, 1);
            currentScale = new Vector3(1, 1, 1);

            Lat0 = MCLatitude;
            Lng0 = MCLongitude;
            Alt0 = MCAltitude;
            lngCorrection = Math.Cos(MCLatitude / 180.0 * Math.PI);
            clipShader = GameObject.FindWithTag("Ground").GetComponent<Renderer>().material.shader;
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
            return ROSCoordToUnityCoord(new GPSCoordinate(gpsPosition.GetLongitude(), gpsPosition.GetLatitude(), gpsPosition.GetAltitude()));
        }

        /// <summary>
        /// Convert the given GPSCoordinate to Unity XYZ space.
        /// </summary>
        /// <param name="gpsPosition"></param>
        /// <returns></returns>
        public static Vector3 ROSCoordToUnityCoord(GPSCoordinate gpsPosition)
        {
            Vector3 unityCoord = Vector3.zero;
            unityCoord.z = (float)((gpsPosition.Lat - Lat0) * EARTH_RADIUS);
            unityCoord.x = (float)((gpsPosition.Lng - Lng0) * EARTH_RADIUS * lngCorrection);
            unityCoord.y = (float)(gpsPosition.Alt - Alt0);

            return unityCoord;
        }

        /// <summary>
        /// Convert Unity coordinates to ROS world lat,long,alt
        /// To be used to convert waypint unity coordinates to world lat.long.alt
        /// </summary>
        /// <param name="unityPosition"></param>
        /// <returns>GPSCoordinates</returns>
        public static GPSCoordinate UnityCoordToROSCoord(Vector3 unityPosition)
        {
            GPSCoordinate gpsCoord = new GPSCoordinate();
            gpsCoord.x = (float)((unityPosition.x / (EARTH_RADIUS * lngCorrection)) + Lng0);
            gpsCoord.y = (float)((unityPosition.z / EARTH_RADIUS) + Lat0);
            gpsCoord.z= (float)((unityPosition.y) + Alt0);
            return gpsCoord;
        }

        /*
        /// <summary>
        ///     Converts the difference between two latitude values to a difference in meters.
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lat2"></param>
        /// <returns>double, difference in meters</returns>
        public static double LatDiffMeters(double lat1, double lat2)
        {
            // assuming earth is a sphere with c = 40075km
            // 1 degree of latitude is = 111.32 km
            //slight inaccuracies
            double delLat = (lat2 - lat1) * 111.32f * 1000;
            //110994.04016313434
            return delLat;
        }

        /// <summary>
        ///     Converts the difference between two longitude values to a difference in meters.
        /// </summary>
        /// <param name="long1"></param>
        /// <param name="long2"></param>
        /// <param name="lat"></param>
        /// <returns>double, distance in meters</returns>
        public static double LongDiffMeters(double long1, double long2, double lat)
        {
            // 1 degree of longitude = 40075 km * cos (lat) / 360
            // we use an arbitrary latitude for the conversion because the difference is minimal 
            //slight inaccuracies
            double delLong = (long2 - long1) * 40075 * (double)Math.Cos(lat) / 360 * 1000;
            return delLong;
        }

        /// <summary>
        ///     Converts the current unity x coordinate to the corresponding latitude for sending waypooints to the drone
        /// </summary>
        /// <param name="lat1"> the home latitude coordinate when the drone first connected to Unity</param>
        /// <param name="unityXCoord">the unity x coordinate for conversion</param>
        /// <returns></returns>
        public static double UnityXToLat(double lat1, float unityXCoord)
        {
            double delLat = (unityXCoord / (1000 * 111.32f) * Unity_X_To_Lat_Scale) + lat1;
            return delLat;
        }

        /// <summary>
        ///     Converts the current unity z coordinate to the corresponding longitude for sending waypoints to the drone.
        /// </summary>
        /// <param name="long1">the home longitude coordinate when the drone first connected to Unity</param>
        /// <param name="lat">the home latitude coordinate when the drone first connected to Unity</param>
        /// <param name="unityZCoord">the unity z coordinate for conversion</param>
        /// <returns></returns>
        public static double UnityZToLong(double long1, double lat, float unityZCoord)
        {
            double delLong = (((unityZCoord * 360) / (1000 * 40075 * (double)Math.Cos(lat))) * Unity_Z_To_Long_Scale) + long1;
            return delLong;
        }

        */
        // Old logic to calculate new position of the drone and for waypoints.
        /// Calculates the 3D displacement of the drone from it's initial position, to its current position, in Unity coordinates.
        /*
        changePos = new Vector3(
            ((float) (WorldProperties.LatDiffMeters(InitialGPSLat, new_ROSPosition._lat)) / WorldProperties.Unity_X_To_Lat_Scale),
                    ((new_ROSPosition._altitude - InitialGPSAlt) / WorldProperties.Unity_Y_To_Alt_Scale),
                    ((float)(WorldProperties.LongDiffMeters(InitialGPSLong, new_ROSPosition._long, new_ROSPosition._lat) / WorldProperties.Unity_Z_To_Long_Scale))
                  );
        /// sets the drone Game Object's local position in the Unity world to be it's start position plus the newly calculated 3d displacement to the drone's current position.
        // Peru 6/9/20: Phasing out World Properties variables used in depreciated script
        // drone.transform.localPosition = WorldProperties.selectedDroneStartPos + offsetPos + changePos;
        */
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
