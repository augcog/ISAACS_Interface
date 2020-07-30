using ISAACS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;


/*
 * This class defines an info panel for a drone. For each drone, the info panel 
 * displays data from subscribers belonging to the drone. The panel follows the 
 * drone in-game and updates in real-time. Data subscribed and displayed may include 
 * battery level, velocity, etc. In addition, there are data not from subscribers
 * which include Drone Name, Lat/Lon, and Authority Status.
 * 
 * The ROS topics that the drone gameobject subscribes to are defined in the World 
 * gameobject in the Unity Editor
 * 
 */

public class DroneMenu : MonoBehaviour {

    // The font used for the menu
    public Font menuFont;
    // The Text UI component attached to the drone prefab for displaying the drone position
    public Text dronePosText;
    // The Text UI component attached to the drone prefab for displaying the drone name
    public Text droneNameText;
    // The Text UI component attached to the drone prefab for displaying the drone authority status
    public Text droneAuthorityText;
    // The Canvas UI component attached to the drone prefab for displaying the drone subscribers data
    public GameObject menuCanvas; // already attached to prefab's canvas
    private ROSDroneConnectionInterface connection; // used to query for data from drone's subscribers
    private List<string> droneSubscriberTopics;
    private Dictionary<Text, string> infoTextsDict; // Holds all Text boxes and their topic names
    private bool initialized;
    private Transform headsetTransform;

    /// <summary>
    /// Creates text box for each subscriber and properly displays them above the drone.
    /// </summary>
    public void InitDroneMenu(ROSDroneConnectionInterface rosDroneConnection, List<string> droneSubscribers)
    {

        connection = rosDroneConnection;
        droneSubscriberTopics = droneSubscribers;
        infoTextsDict = new Dictionary<Text, string>();

        // Creating Text label for each subscriber of the drone
        float offsetY = -190; // text position displacement from anchor location (top center)
        offsetY += droneSubscribers.Count * 60; // displacing top text position based on num subscribers
        droneNameText.rectTransform.anchoredPosition = new Vector3(0, offsetY, 0); // top text box is drone name
        droneNameText.text = this.name;

        foreach (string subscriber in droneSubscribers)
        {
            offsetY -= 60f;

            // create text box
            GameObject placeholder = new GameObject("text of " + subscriber);
            placeholder.transform.SetParent(menuCanvas.transform);
            placeholder.transform.localScale = new Vector3(1, 1, 1);

            //text location
            Text infoText = placeholder.AddComponent<Text>();
            infoText.rectTransform.anchorMax = new Vector2(0.5f, 1); // top-center anchor location
            infoText.rectTransform.anchorMin = new Vector2(0.5f, 1); // top-center anchor location
            infoText.rectTransform.anchoredPosition = new Vector3(0, offsetY, 0);

            // text content & stylizing
            infoText.text = "Lorem Ipsum";
            infoText.alignment = TextAnchor.MiddleCenter;
            infoText.verticalOverflow = VerticalWrapMode.Overflow;
            infoText.horizontalOverflow = HorizontalWrapMode.Overflow;
            infoText.font = menuFont;
            infoText.fontSize = 80;
            infoText.fontStyle = FontStyle.Bold;
            infoText.color = Color.green;

            infoTextsDict.Add(infoText, subscriber);  
        }

        initialized = true;
    }


    /// <summary>
    /// Updates data of each text box
    /// </summary>e
    void Update() {
        if (initialized)
        {
            foreach (KeyValuePair<Text, string> item in infoTextsDict)
            {
                item.Key.text = connection.GetValueByTopic(item.Value);
            }

            Vector3 dronePosition = this.gameObject.transform.localPosition;
            GPSCoordinate gps =  WorldProperties.UnityCoordToGPSCoord(dronePosition);
            dronePosText.text = "Lat:   " + String.Format("{0:0.0000000}", gps.Lat) + "\nLon: " + String.Format("{0:0.0000000}", gps.Lng);

            if (connection.HasAuthority())
            {
                droneAuthorityText.text = "Controllable";
                droneAuthorityText.color = Color.green;
            }
            else
            {
                droneAuthorityText.text = "Request Authority";
                droneAuthorityText.color = Color.white;
            }

            // Make canvas always face the user as drone moves
            headsetTransform = VRTK_DeviceFinder.HeadsetTransform();
            if (headsetTransform != null)
            {
                Vector3 targetPosition = headsetTransform.position;
                // canvas LookAt code is funky. Credit: https://answers.unity.com/questions/132592/lookat-in-opposite-direction.html
                menuCanvas.transform.LookAt(2 * menuCanvas.transform.position - targetPosition);
            }
        }
    }



}
