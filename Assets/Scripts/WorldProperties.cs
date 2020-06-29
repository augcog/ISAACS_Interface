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
        private static Drone selectedDrone;
        private static char nextDroneId;
        private static Dictionary<char, Drone> dronesDict;
        private static Queue<Drone> dronesQueue;

        [Header("Sensor vairables")]
        private static GameObject selectedSensor;
        private static Dictionary<int, GameObject> sensorDict;

        [Header(" Misc. State variables")]
        public static GameObject worldObject;
        public static GameObject placementPlane;

        public static Vector3 actualScale;
        public static Vector3 currentScale;

        private static Shader clipShader;

        [Header("Unity-ROS Conversion Variables")]
        // ROS-Unity conversion variables
        private static float earth_radius = 6378137;
        private static Vector3 initial_DroneROS_Position = Vector3.zero;
        private static Vector3 initial_DroneUnity_Position = Vector3.zero;
        private static float ROS_to_Unity_Scale = 0.0f;

        //Unity to lat --> multiply by scale; lat to Unity --> divide by scale
        private static float Unity_X_To_Lat_Scale = 10.0f;
        private static float Unity_Y_To_Alt_Scale = 10.0f;
        private static float Unity_Z_To_Long_Scale = 10.0f;

        // Use this for initialization
        void Start()
        {
            selectedDrone = null;
            dronesDict = new Dictionary<char, Drone>(); // Collection of all the drone classObjects
            dronesQueue = new Queue<Drone>();

            selectedSensor = null;
            sensorDict = new Dictionary<int, GameObject>();

            nextDroneId = 'A'; // Used as an incrementing key for the dronesDict and for a piece of the communication about waypoints across the ROSBridge

            worldObject = gameObject;

            placementPlane = GameObject.FindWithTag("Ground");

            actualScale = new Vector3(1, 1, 1);
            currentScale = new Vector3(1, 1, 1);

            clipShader = GameObject.FindWithTag("Ground").GetComponent<Renderer>().material.shader;
        }

        /// <summary>
        /// Cycle through the connected drones
        /// </summary>
        public static void SelectNextDrone()
        {
            Debug.Log("Selection next drone");

            if(selectedDrone != null)
            {
                dronesQueue.Enqueue(selectedDrone);
            }

            if (dronesQueue.Count > 0)
            {
                Drone nextDrone = dronesQueue.Dequeue();
                nextDrone.gameObjectPointer.GetComponent<DroneProperties>().SelectDrone();
            }
            else
            {
                Debug.Log("No drones connected");
            }

        }

        /// <summary> 
        /// Get nextDroneID 
        /// </summary>
        public static char GetNextDroneID()
        {
            return nextDroneId;
        }
        
        /// <summary>
        /// Increment the next drone ID
        /// </summary>
        public static void IncrementDroneID()
        {
            nextDroneId++;
        }

        /// <summary>
        /// Update the selected drone.
        /// </summary>
        public static void UpdateSelectedDrone(Drone newSelectedDrone)
        {
            if (selectedDrone != null)
            {
                selectedDrone.gameObjectPointer.GetComponent<DroneProperties>().DeselectDrone();
            }
            selectedDrone = newSelectedDrone;
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
        /// <param name="id"></param>
        /// <param name="drone"></param>
        public static void AddDrone(char id, Drone drone)
        {
            dronesDict.Add(id, drone);
            dronesQueue.Enqueue(drone);
        }

        /// <summary>
        /// Get list of all drones
        /// </summary>
        public static Dictionary<char, Drone> GetDronesDict()
        {
            return dronesDict;
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


        // TODO: Complete these functions

        /// <summary>
        /// Convert the Given ROS NavSatFixMsg to Unity XYZ space.
        /// To Be used to convert drone coordinates to unity space
        /// </summary>
        /// <returns></returns>
        public static Vector3 ROSCoordToUnityCoord(NavSatFixMsg gpsPosition)
        {


            return Vector3.zero;
        }

        /// <summary>
        /// Convert Unity coordinates to ROS world lat,long,alt
        /// To be used to convert waypint unity coordinates to world lat.long.alt
        /// </summary>
        /// <param name="unityPosition"></param>
        /// <returns></returns>
        public static NavSatFixMsg UnityCoordToROSCoord(Vector3 unityPosition)
        {
            return null;
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

        /// <summary>
        /// Converts unity Y coordinate to Altitude
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float UnityYtoAlt(float y)
        {
            return (y * WorldProperties.Unity_Y_To_Alt_Scale) - 1f;
        }

        /// <summary>
        /// Query UnityX to World Lat scale
        /// </summary>
        /// <returns></returns>
        public static float GetUnityXtoLatScale()
        {
            return Unity_X_To_Lat_Scale;
        }

        /// <summary>
        /// Query UnityY to World Alt scale
        /// </summary>
        /// <returns></returns>
        public static float GetUnityYtoAltScale()
        {
            return Unity_Y_To_Alt_Scale;
        }

        /// <summary>
        /// Query UnityZ to World Long scale
        /// </summary>
        /// <returns></returns>
        public static float GetUnityZtoLongScale()
        {
            return Unity_Z_To_Long_Scale;
        }
        
        // Old logic to calculate new position of the drone, will implement in after merge.
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
}
