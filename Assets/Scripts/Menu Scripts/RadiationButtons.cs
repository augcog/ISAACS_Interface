using UnityEngine;
using UnityEngine.UI; // <-- you need this to access UI (button in this case) functionalities
using UnityEngine.SceneManagement;
using System.Collections;
using VRTK;
using ISAACS;

public class RadiationButtons : MonoBehaviour {

    Button myButton;

    public bool surface_pointcloud = false;
    public bool Level_0 = false;
    public bool Level_1 = false;
    public bool Level_2 = false;
    public bool Level_3 = false;
    public bool Level_4 = false;
    public bool Level_5 = false;

    void Awake()
    {
        myButton = GetComponent<Button>(); // <-- you get access to the button component here
        myButton.onClick.AddListener(() => { OnClickEvent(); });  // <-- you assign a method to the button OnClick event here
    }

    void OnClickEvent()
    {
        if (surface_pointcloud)
        {
            Debug.Log("Switching to surface point cloud");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            //lampSensor_ROS.Unsubscribe();
        }

        if (Level_0)
        {
            Debug.Log("Switching to Level 0");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            //lampSensor_ROS.Unsubscribe();
        }

        if (Level_1)
        {
            Debug.Log("Switching to Level 1");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            //lampSensor_ROS.Unsubscribe();
        }

        if (Level_2)
        {
            Debug.Log("Switching to Level 2");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            //lampSensor_ROS.Unsubscribe();
        }


        if (Level_3)
        {
            Debug.Log("Switching to Level 3");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            //lampSensor_ROS.Unsubscribe();
        }

        if (Level_4)
        {
            Debug.Log("Switching to Level 4");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
            //lampSensor_ROS.Unsubscribe();
        }

        if (Level_5)
        {
            Debug.Log("Switching to Level 5");
            GameObject sensor = WorldProperties.GetSelectedSensor();
            LampSensor_ROSSensorConnection lampSensor_ROS = sensor.GetComponent<LampSensor_ROSSensorConnection>();
        }

    }
}
