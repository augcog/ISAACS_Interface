using ISAACS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using static ROSManager;

public class DroneMenu : MonoBehaviour {

    public Font menuFont;
    public Text dronePosText;
    public Text droneNameText;
    public Text droneAuthorityText;
    public GameObject menuCanvas; // already attached to prefab's canvas
    bool hasAuthority;
    ROSDroneConnectionInterface connection;
    private List<string> droneSubscriberTopics;
    Dictionary<Text, string> infoTextsDict; // Holds all Text boxes and their topic names
    private bool initialized;
    private Transform headset;


    public void InitDroneMenu(ROSDroneConnectionInterface rosDroneConnection, List<string> droneSubscribers)
    {
        connection = rosDroneConnection;
        droneSubscriberTopics = droneSubscribers;
        infoTextsDict = new Dictionary<Text, string>();
        
        // Creating Text label for each subscriber of the drone
        float offsetY = -320f; // text position displacement from anchor location (top center)
        foreach (string subscriber in droneSubscribers)
        {
            Debug.Log("Adding text field for subscriber: ," + subscriber);
            GameObject placeholder = new GameObject("text of " + subscriber);
            placeholder.transform.SetParent(menuCanvas.transform);
            placeholder.transform.localScale = new Vector3(1, 1, 1);

            //text location
            Text infoText = placeholder.AddComponent<Text>();
            infoText.rectTransform.anchorMax = new Vector2(0.5f, 1); // top center anchor location
            infoText.rectTransform.anchorMin = new Vector2(0.5f, 1); // top center anchor location
            infoText.rectTransform.anchoredPosition = new Vector3(0, offsetY, 0);

            // text content & stylizing
            infoText.text = "Test123"; //connection.GetValueByTopic(subscriber);
            infoText.alignment = TextAnchor.MiddleCenter;
            infoText.verticalOverflow = VerticalWrapMode.Overflow;
            infoText.horizontalOverflow = HorizontalWrapMode.Overflow;
            infoText.font = menuFont;
            infoText.fontSize = 80;
            infoText.fontStyle = FontStyle.Bold;
            infoText.color = Color.green;

            infoTextsDict.Add(infoText, subscriber);
            offsetY -= 60f;
        }

        droneNameText.text = this.name;
        initialized = true;
    }


    // Update is called once per frame
    void Update() {
        if (initialized)
        {

            Vector3 dronePosition = this.gameObject.transform.localPosition;
            Debug.Log("Drone Pos: " + dronePosition);
            foreach (KeyValuePair<Text, string> item in infoTextsDict)
            {
                item.Key.text = connection.GetValueByTopic(item.Value);
            }


            // Original Drone Coords
            //37.915345, -122.337932

            GPSCoordinate gps =  WorldProperties.UnityCoordToROSCoord(dronePosition);
            //TODO pocisiton can be its own auto text field

            dronePosText.text = "Lat:   " + String.Format("{0:0.0000000}", gps.Lat) + "\nLon: " + String.Format("{0:0.0000000}", gps.Lng);

            hasAuthority = connection.HasAuthority();
            if (hasAuthority)
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
            headset = VRTK_DeviceFinder.HeadsetTransform();
            Vector3 targetPosition = headset.position;
            // canvas LookAt is funky. Credit: https://answers.unity.com/questions/132592/lookat-in-opposite-direction.html
            menuCanvas.transform.LookAt(2 * menuCanvas.transform.position - targetPosition);
        }
    }



}
