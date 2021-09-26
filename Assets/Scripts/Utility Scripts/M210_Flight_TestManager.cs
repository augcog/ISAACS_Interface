using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using UnityEditor;
using System.IO;
using ISAACS;
using ROSBridgeLib.sensor_msgs;

public class M210_Flight_TestManager : MonoBehaviour
{

    /// <summary>
    /// This script is for all debugging using keyboard triggers. 
    /// Each string maps a key to the required function used for debugging.
    /// </summary>

    [Header("Initialize")]
    public string init = "0";

    [Header("Subscriber Tests")]
    public string viewHomeLat = "q";
    public string viewHomeLong = "w";
    public bool printLatLong = false;
    public bool printGPSHealth = false;

    [Header("Authority and KillSwitch Tests")]
    public string getAuthority = "e";
    public string hasAuthority = "r";
    public string killSwitch = "SPACE";

    [Header("Basic Motor Commands Test")]
    public string spinMotors = "y";
    public string stopMotors = "u";

    [Header("Takeoff/Land Tests")]
    public string takeoffDrone = "i";
    public string landDrone = "o";

    [Header("Safe Mission Test (Hardcoded)")]
    public string viewSafeWaypointMission = "d";
    public string uploadTestMission = "p";
    public string missionInfo = "a";
    
    [Header("RFS B100 Mapping Test (Hardcoded)")]
    public string uploadRFS_B100MappingMission = "m";

    [Header("Safe Mission Test (User Set)")]
    public string uploadUserMission = "f";
    public string executeUploadedMission = "s";

    [Header("Pause/Resume Mission Tests")]
    public string pauseMission = "g";
    public string resumeMission = "h";

    [Header("Drone/Sensor Selection Tests")]
    public string cycleDrone = "j";
    public string cycleSensor = "k";
    public string unsubscribe = "z";

    [Header("Dyanmic waypoint system Tests")]
    // TODO: Ensure waypoint array removes past waypoints.
    public string stopMission = "l";

    [Header("LAMP radiation type")]
    public string selectGamma = "z";
    public string selectNeutron = "x";

    [Header("Drone Variable")]
    public Matrice_ROSDroneConnection rosDroneConnection;

    /// <summary>
    /// Set the rosDroneConnection to given argument
    /// </summary>
    /// <param name="new_ROSDroneConnection"></param>
    public void UpdateDrone(ROSDroneConnectionInterface new_ROSDroneConnection)
    {
        rosDroneConnection = (Matrice_ROSDroneConnection)new_ROSDroneConnection;
    }

    public void ViewHardcodedWaypoints(MissionWaypointTaskMsg taskMsg)
    {
        int wpNum = 1;
        foreach (MissionWaypointMsg wp in taskMsg.GetMissionWaypoints()){

            GameObject waypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypoint.name = "Waypoint " + wpNum;
            waypoint.transform.parent = this.transform;
            waypoint.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(wp.GetLatitude(), wp.GetLongitude(), wp.GetAltitude()));
            waypoint.transform.localScale = Vector3.one * (0.2f);
            wpNum++;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(init))
        {
            Drone drone = WorldProperties.SelectNextDrone();
            rosDroneConnection = (Matrice_ROSDroneConnection)drone.droneProperties.droneROSConnection;
        }

        if (Input.GetKeyDown(viewHomeLat))
        {
            double homeLat = rosDroneConnection.GetHomeLat();
            Debug.LogFormat("Home Latitute set to {0}", homeLat);
        }

        if (Input.GetKeyDown(viewHomeLong))
        {
            double homeLong = rosDroneConnection.GetHomeLong();
            Debug.LogFormat("Home Longitute set to {0}", homeLong);
        }

        if (printLatLong)
        {
            NavSatFixMsg gpsPosition = rosDroneConnection.GetGPSPosition();
            Debug.LogFormat("Drone Lat,Long : {0},{1}", gpsPosition.GetLatitude(), gpsPosition.GetLongitude());
        }

        if (printGPSHealth)
        {
            float gpsHealth = rosDroneConnection.GetGPSHealth();
            Debug.LogFormat("GPS Health : {0}", gpsHealth);
        }

        if (Input.GetKeyUp(hasAuthority))
        {
            rosDroneConnection.HasAuthority();
        }

        if (Input.GetKeyUp(getAuthority))
        {
            rosDroneConnection.SetSDKControl(true);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            rosDroneConnection.SetSDKControl(false);
        }

        if (Input.GetKeyDown(spinMotors))
        {
            rosDroneConnection.ChangeArmStatusTo(true);
        }

        if (Input.GetKeyDown(stopMotors))
        {
            rosDroneConnection.ChangeArmStatusTo(false);
        }

        if (Input.GetKeyUp(takeoffDrone))
        {
            rosDroneConnection.ExecuteTask(Matrice_ROSDroneConnection.DroneTask.TAKEOFF);
        }

        if (Input.GetKeyUp(landDrone))
        {
            rosDroneConnection.ExecuteTask(Matrice_ROSDroneConnection.DroneTask.LAND);
        }

        if (Input.GetKeyUp(uploadTestMission))
        {
            uint[] command_list = new uint[16];
            uint[] command_params = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                command_list[i] = 0;
                command_params[i] = 0;
            }

            MissionWaypointMsg test_waypoint_1 = new MissionWaypointMsg(37.915701652f, -122.337967237f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
            MissionWaypointMsg test_waypoint_2 = new MissionWaypointMsg(37.915585270f, -122.338122805f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
            MissionWaypointMsg test_waypoint_3 = new MissionWaypointMsg(37.915457249f, -122.338015517f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
            Debug.Log("Check float accuracy here" + test_waypoint_1.ToYAMLString());

            MissionWaypointMsg[] test_waypoint_array = new MissionWaypointMsg[] { test_waypoint_1, test_waypoint_2, test_waypoint_3 };

            MissionWaypointTaskMsg test_Task = new MissionWaypointTaskMsg(15.0f, 15.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.COORDINATED, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, test_waypoint_array);
            
            rosDroneConnection.UploadWaypointsTask(test_Task);

        }

        // This is for flying around B100 for mapping it
        // If yaw_turn_mode = True Untested: so ENSURE drone follows YAW angles correctly
        // If yaw_turn_mode = False, drone flies a regular mission, always pointing in the direction of flight. Make sure camera is mounted to the drone
        // using the mount that makes the camera pointed to the left.
        if (Input.GetKeyUp(uploadRFS_B100MappingMission))
        {
            uint[] command_list = new uint[16];
            uint[] command_params = new uint[16];
            bool yaw_turn_mode = false;
            for (int i = 0; i < 16; i++)
            {
                command_list[i] = 0;
                command_params[i] = 0;
            }

            // Yaw angle (degrees) is probably NED (North East Down)
            // If so, 0 deg = North, 90 deg = East
            //         N 0
            //     315     45
            // W 270  *B100*  E 90
            //     225     135
            //        S 180
            // 

            MissionWaypointTaskMsg mapB100Task = null;

            // UNTESTED WPs. Not considered safe until tested.
            //3D Mapping hardcoded yaw - adjusted WPs for mapping B100, for a drone outfitted with a forward - facing camera
            // Make sure drone's camera is pointing forward as drone is supposed to turn (yaw changing) to always face the building.

                    if (yaw_turn_mode)
            {
                // flight path clockwise around B100 defined below:
                // Start out in the field
                MissionWaypointMsg mappingWP1 = new MissionWaypointMsg(37.915373f, -122.337945f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //NE Corner of B100 
                MissionWaypointMsg mappingWP2 = new MissionWaypointMsg(37.915279f, -122.337941f, 20.0f, 3.0f, 225, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //SE Corner of B100 
                MissionWaypointMsg mappingWP3 = new MissionWaypointMsg(37.915152f, -122.337943f, 20.0f, 3.0f, 315, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //SW Corner of B100
                MissionWaypointMsg mappingWP4 = new MissionWaypointMsg(37.915153f, -122.338110f, 20.0f, 3.0f, 45, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //NW Corner of B100
                MissionWaypointMsg mappingWP5 = new MissionWaypointMsg(37.915291f, -122.338112f, 20.0f, 3.0f, 135, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //NE Corner of B100 
                MissionWaypointMsg mappingWP6 = new MissionWaypointMsg(37.915279f, -122.337941f, 20.0f, 3.0f, 225, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                // Back to out in the field
                MissionWaypointMsg mappingWP7 = new MissionWaypointMsg(37.915373f, -122.337945f, 20.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                MissionWaypointMsg[] mappingWPs = new MissionWaypointMsg[] { mappingWP1, mappingWP2, mappingWP3, mappingWP4, mappingWP5, mappingWP6, mappingWP7 };
                mapB100Task = new MissionWaypointTaskMsg(8.0f, 4.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.WAYPOINT, MissionWaypointTaskMsg.TraceMode.COORDINATED, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, mappingWPs);
                ViewHardcodedWaypoints(mapB100Task);
                // Replace with below line for point-to-point transition between WPs instead of smooth (idk what it does)
                // MissionWaypointTaskMsg test_Task = new MissionWaypointTaskMsg(8.0f, 4.0f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.WAYPOINT, MissionWaypointTaskMsg.TraceMode.POINT, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, mappingWPs);

            }

            // Flies Counter clockwise (CCW) around B100 without any fancy yaw -- drone always points in the direction of flight, as is usual
            // These WPs lat/long are safe. Fly at high altitude to verify they're good. They also show up in Unity so you can see where they
            // are in relation to the building. I've flown with these lat/long through iPAD DJI GS Pro app, and then compied the lat/long values
            // to here so they are supposed to be safe if you use these. Just set the altitude to your preference. 
            else if (yaw_turn_mode == false)
            {
                // flight path ***COUNTER-CLOCKWISE*** around B100 defined below:
                // Start out next to building. Have drone hovering close to this WP when starting mapping routine on jetson computer.
                //NW Corner of B100
                MissionWaypointMsg mappingWP1a = new MissionWaypointMsg(37.915306300f, -122.338041512f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
                MissionWaypointMsg mappingWP1b = new MissionWaypointMsg(37.915266382f, -122.338096355f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //SW Corner of B100 
                MissionWaypointMsg mappingWP2a = new MissionWaypointMsg(37.915208625f, -122.338096832f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
                MissionWaypointMsg mappingWP2b = new MissionWaypointMsg(37.915168440f, -122.338051218f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //SE Corner of B100
                MissionWaypointMsg mappingWP3a = new MissionWaypointMsg(37.915165315f, -122.337991285f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
                MissionWaypointMsg mappingWP3b = new MissionWaypointMsg(37.915202140f, -122.337917794f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //NE Corner of B100
                MissionWaypointMsg mappingWP4a = new MissionWaypointMsg(37.915273840f, -122.337920489f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));
                MissionWaypointMsg mappingWP4b = new MissionWaypointMsg(37.915327647f, -122.337965254f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                //NW Corner of B100 
                MissionWaypointMsg mappingWP5 = new MissionWaypointMsg(37.915326300f, -122.338047125f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                // Back to out in the field
                MissionWaypointMsg mappingWP6 = new MissionWaypointMsg(37.915387568f, -122.337874484f, 10.0f, 3.0f, 0, 0, MissionWaypointMsg.TurnMode.CLOCKWISE, 0, 30, new MissionWaypointActionMsg(0, command_list, command_params));

                MissionWaypointMsg[] mappingWPs = new MissionWaypointMsg[] {
                    mappingWP1a,
                    mappingWP1b,
                    mappingWP2a,
                    mappingWP2b,
                    mappingWP3a,
                    mappingWP3b,
                    mappingWP4a,
                    mappingWP4b,
                    mappingWP5,
                    mappingWP6};
                mapB100Task = new MissionWaypointTaskMsg(0.5f, 0.5f, MissionWaypointTaskMsg.ActionOnFinish.NO_ACTION, 1, MissionWaypointTaskMsg.YawMode.AUTO, MissionWaypointTaskMsg.TraceMode.COORDINATED, MissionWaypointTaskMsg.ActionOnRCLost.FREE, MissionWaypointTaskMsg.GimbalPitchMode.FREE, mappingWPs);
                ViewHardcodedWaypoints(mapB100Task);
            }

            rosDroneConnection.UploadWaypointsTask(mapB100Task);
        }


        if (Input.GetKeyUp(missionInfo))
        {
            rosDroneConnection.FetchMissionStatus();
        }

        if (Input.GetKeyUp(executeUploadedMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.START);
        }

        if (Input.GetKeyUp(viewSafeWaypointMission))
        {


            // 37.915411
            // -122.338024

            // 37.915159, -122.337983


            // LEFT CORNER:  37.915188, -122.337988
            // RIGHT CORNER: 37.915262, -122.337984


            GameObject waypoint1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypoint1.name = "Waypoint 1";
            waypoint1.transform.parent = this.transform;
            waypoint1.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915701652, -122.337967237, 20));
            waypoint1.transform.localScale = Vector3.one * (0.1f);

            Debug.Log("SANITY CHECK GPS: " + WorldProperties.UnityCoordToGPSCoord( WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915701652, -122.337967237, 20))).Lat);
            Debug.Log("SANITY CHECK GPS: " + WorldProperties.UnityCoordToGPSCoord(WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915701652, -122.337967237, 20))).Lng);
            GameObject waypoint2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypoint2.name = "Waypoint 2";
            waypoint2.transform.parent = this.transform;
            waypoint2.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915585270, -122.338122805, 20));
            waypoint2.transform.localScale = Vector3.one * (0.1f);

            GameObject waypoint3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waypoint3.name = "Waypoint 3";
            waypoint3.transform.parent = this.transform;
            waypoint3.transform.localPosition = WorldProperties.GPSCoordToUnityCoord(new GPSCoordinate(37.915457249, -122.338015517, 20));
            waypoint3.transform.localScale = Vector3.one * (0.1f);
        }

        if (Input.GetKeyUp(uploadUserMission))
        {
            rosDroneConnection.StartMission();
        }

        if (Input.GetKeyUp(pauseMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.PAUSE);
        }

        if (Input.GetKeyUp(resumeMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.RESUME);
        }

        if (Input.GetKeyDown(stopMission))
        {
            rosDroneConnection.SendWaypointAction(Matrice_ROSDroneConnection.WaypointMissionAction.STOP);
        }

        if (Input.GetKeyUp(cycleDrone))
        {
            Drone drone = WorldProperties.SelectNextDrone();
            rosDroneConnection = (Matrice_ROSDroneConnection)drone.droneProperties.droneROSConnection;
        }

        if (Input.GetKeyDown(cycleSensor))
        {
            WorldProperties.sensorManager.ShowNextSensor();
        }

        if (Input.GetKeyDown(unsubscribe))
        {
            ROSSensorConnectionInterface sensor = WorldProperties.sensorManager.getSelectedSensor();
            Dictionary<string, bool> subscriberDict = WorldProperties.sensorManager.getSubscriberDict();
            foreach (string topic in subscriberDict.Keys)
            {
                sensor.Unsubscribe(topic);
            }
        }

        if (Input.GetKeyDown(selectGamma))
        {
            WorldProperties.radiationDataType = WorldProperties.RadiationDataType.gamma;
        }

        if (Input.GetKeyDown(selectNeutron))
        {
            WorldProperties.radiationDataType = WorldProperties.RadiationDataType.neutron;
        }

    }
}
