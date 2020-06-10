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

    // TODO: we might want to lock FPS!
    /// <summary>
    /// This is the only class that should have static variables or functions that are consistent throughout the entire program.
    /// </summary>
    public class WorldProperties : MonoBehaviour
    {
        [Header("Required Prefabs")]
        public GameObject droneBaseObject;
        public GameObject waypointBaseObject;

        [Header("Drone state variables")]
        public static Drone selectedDrone;
        public static char nextDroneId;
        public static Dictionary<char, Drone> dronesDict;

        [Header(" Misc. State variables")]
        public static GameObject worldObject;
        public static GameObject placementPlane;

        public static Vector3 actualScale;
        public static Vector3 currentScale;

        public static Shader clipShader;

        [Header("Unity-ROS Conversion Variables")]
        // ROS-Unity conversion variables
        public static float earth_radius = 6378137;
        public static Vector3 initial_DroneROS_Position = Vector3.zero;
        public static Vector3 initial_DroneUnity_Position = Vector3.zero;
        public static float ROS_to_Unity_Scale = 0.0f;

        //Unity to lat --> multiply by scale; lat to Unity --> divide by scale
        public static float Unity_X_To_Lat_Scale = 10.0f;
        public static float Unity_Y_To_Alt_Scale = 10.0f;
        public static float Unity_Z_To_Long_Scale = 10.0f;

        // Use this for initialization
        void Start()
        {
            selectedDrone = null;
            dronesDict = new Dictionary<char, Drone>(); // Collection of all the drone classObjects

            nextDroneId = 'A'; // Used as an incrementing key for the dronesDict and for a piece of the communication about waypoints across the ROSBridge

            worldObject = gameObject;

            placementPlane = GameObject.FindWithTag("Ground");

            actualScale = new Vector3(1, 1, 1);
            currentScale = new Vector3(1, 1, 1);

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
    }
}
