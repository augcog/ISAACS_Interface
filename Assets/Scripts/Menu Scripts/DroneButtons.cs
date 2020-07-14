using UnityEngine;
using UnityEngine.UI; // <-- you need this to access UI (button in this case) functionalities
using UnityEngine.SceneManagement;
using System.Collections;
using VRTK;
using ISAACS;

public class DroneButtons : MonoBehaviour {

    Button myButton;

    public bool startMission = false;
    public bool pauseMission = false;
    public bool resumeMission = false;
    public bool clearWaypoints = false;
    public bool landDrone = false;
    public bool homeDrone = false;


    void Awake()
    {
        myButton = GetComponent<Button>(); // <-- you get access to the button component here
        myButton.onClick.AddListener(() => { OnClickEvent(); });  // <-- you assign a method to the button OnClick event here
    }

    void OnClickEvent()
    {
        Drone selectedDrone = WorldProperties.GetSelectedDrone();
        ROSDroneConnectionInterface droneROSConnection = selectedDrone.droneProperties.droneROSConnection;

        if (startMission)
        {
            droneROSConnection.StartMission();
        }

        if (pauseMission)
        {
            droneROSConnection.PauseMission();
        }

        if (resumeMission)
        {
            droneROSConnection.ResumeMission();
        }


        if (clearWaypoints)
        {
            selectedDrone.DeleteAllWaypoints();
        }


        if (landDrone)
        {
            droneROSConnection.LandDrone();
        }


        if (homeDrone)
        {
            droneROSConnection.FlyHome();        
        }


    }
}
