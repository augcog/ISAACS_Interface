using UnityEngine;
using UnityEngine.UI; // <-- you need this to access UI (button in this case) functionalities
using UnityEngine.SceneManagement;
using System.Collections;
using VRTK;
using ISAACS;

public class DroneButtons : MonoBehaviour {

    Button myButton;
    Drone drone;
    private GameObject controller; //needed to access pointer

    public bool simulation = true;

    public bool startMission = false;
    public bool pauseMission = false;
    public bool resumeMission = false;
    public bool clearWaypoints = false;
    public bool landDrone = false;
    public bool homeDrone = false;


    void Awake()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");

        myButton = GetComponent<Button>(); // <-- you get access to the button component here
        myButton.onClick.AddListener(() => { OnClickEvent(); });  // <-- you assign a method to the button OnClick event here
    }

    void OnClickEvent()
    {
        if (startMission)
        {
            Debug.Log("Start Mission Button: Currently not connected to Drone_ROS Connection");

            if (simulation)
            {
                GameObject world = GameObject.FindGameObjectWithTag("World");
                DroneSimulationManager droneSim = world.GetComponent<DroneSimulationManager>();
                droneSim.FlyNextWaypoint(true);
                return;
            }

        }

        if (pauseMission)
        {
            Debug.Log("TO TEST: Pause Mission Button: Currently not connected to Drone_ROS Connection");

            if (simulation)
            {
                GameObject world = GameObject.FindGameObjectWithTag("World");
                DroneSimulationManager droneSim = world.GetComponent<DroneSimulationManager>();
                droneSim.pauseFlight();
                return;
            }
            // Test and switch
            // WorldProperties.PauseDroneMission();
        }

        if (resumeMission)
        {
            Debug.Log("TO TEST: Resume Mission Button: Currently not connected to Drone_ROS Connection");
            if (simulation)
            {
                GameObject world = GameObject.FindGameObjectWithTag("World");
                DroneSimulationManager droneSim = world.GetComponent<DroneSimulationManager>();
                droneSim.resumeFlight();
                return;
            }

            // Test and switch
            // WorldProperties.ResumeDroneMission();
        }


        if (clearWaypoints)
        {
            Debug.Log(" Clear Waypoints  Button");
            if (controller.GetComponent<VRTK_Pointer>().IsPointerActive())
            {
                drone = WorldProperties.GetSelectedDrone();
                while (drone.waypoints.Count > 1)
                {
                    if (((Waypoint)drone.waypoints[drone.waypoints.Count - 1]).prevPathPoint != null)
                    {
                        drone.DeleteWaypoint((Waypoint)drone.waypoints[drone.waypoints.Count - 1]);
                    }
                }
            }
        }


        if (landDrone)
        {
            Debug.Log("Land  Button: Currently not connected to Drone_ROS Connection");
            //WorldProperties.worldObject.GetComponent<ROSDroneConnection>().Land();
        }


        if (homeDrone)
        {
            Debug.Log("Home  Button: Currently not connected to Drone_ROS Connection");

            if (simulation)
            {
                GameObject world = GameObject.FindGameObjectWithTag("World");
                DroneSimulationManager droneSim = world.GetComponent<DroneSimulationManager>();
                droneSim.flyHome();
                return;
            }

            //WorldProperties.worldObject.GetComponent<ROSDroneConnection>().GoHome();
        }


    }
}
