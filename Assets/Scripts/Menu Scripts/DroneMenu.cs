using ISAACS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ROSManager;

public class DroneMenu : MonoBehaviour {

    public Text dronePosText;
    public Text droneNameText;
    public Text droneAuthorityText;
    public GameObject menuCanvas; // already attaached to prefab's canvas
    double homeLat;
    double homeLong;
    bool hasAuthority;
    Drone drone;
    private List<string> droneSubscribers;
    private bool initialized;
    ROSDroneConnectionInterface connection;
    // c# dictionary to hold list of text fields.

    // Use this for initialization
    //void Start () {
    //f = GameObject.Find("DronePos");
    //Debug.Log(f);

    // TODO: MAKE MOST/ALL find be Transfomr.find which looks in children since transform means current GO. It's not just XYZ, pose, etc.

    //menuCanvas = GameObject.Find("DroneMenuCanvas"); // does this select a canvas of current drone, or of all instances?
    //menuCanvas.transform.SetParent(this.gameObject.transform);
    //float scale = 0.00361908f;
    //menuCanvas.transform.localScale = new Vector3(scale, scale, scale);
    //menuCanvas.transform.localPosition = new Vector3(0.0f, 0.37f, 0.0f);

    /*
    dronePosText = GameObject.Find("DronePos").GetComponent<Text>();
    droneNameText = GameObject.Find("DroneName").GetComponent<Text>();
    droneAuthorityText = GameObject.Find("DroneAuthority").GetComponent<Text>(); // replace with find in children
    */



    // drone -> Matric connection -> get home lat

    //}

    // Update is called once per frame
    void Update() {
        if (initialized)
        {
            Vector3 dronePosition = this.gameObject.transform.position;

            //menuCanvas.transform.LookAt(GameObject.Find("OVRCameraRig").transform); // TODO: make menu always face the user


            foreach (string subscriber in droneSubscribers)
            {
                Text entry = menuCanvas.AddComponent<Text>();
                entry.text = connection.GetTopicValue(subscriber);
                //entry.transform.position
            }

            //37.915345, -122.337932
            //double lat = WorldProperties.UnityXToLat(37.915345, dronePosition.x); // HARDCODED
            double lat = 0.0f;
            double lon = 0.0f;
            //double lon = WorldProperties.UnityZToLong(-122.337932, lat, dronePosition.z); // HARDCODED
            //double lat = WorldProperties.UnityXToLat(homeLat, dronePosition.x);
            //double lon = WorldProperties.UnityZToLong(homeLong, lat, dronePosition.z);

            dronePosText.text = "Lat:   " + String.Format("{0:0.0000000}", lat) + "\nLon: " + String.Format("{0:0.0000000}", lon);
            droneNameText.text = this.name;
            if (hasAuthority)
            {
                droneAuthorityText.text = "Controllable";
                droneAuthorityText.color = Color.green;
            } else
            {
                droneAuthorityText.text = "Request Authority";
                droneAuthorityText.color = Color.white;
            }
        }
    }


    public void InitDroneMenu(Drone drone, List<string> droneSubscribers)
    {
        this.drone = drone;
        this.droneSubscribers = droneSubscribers;


        foreach (string subscriber in droneSubscribers)
        {
            Text entry = menuCanvas.AddComponent<Text>();
            entry.text = "Test123";
            //entry.transform.position
        }
        //GameObject newGO = new GameObject("myTextGO");
        //ngo.transform.SetParent(this.transform);

        //Text myText = ngo.AddComponent<Text>();
        //myText.text = "Ta-dah!";

        connection = this.GetComponent<DroneProperties>().droneROSConnection;

        homeLat = connection.GetHomeLat(); // dronepropertieis.rosdroneconnection to allow 100/210/600
        homeLong = connection.GetHomeLong();
        hasAuthority = connection.HasAuthority();

        initialized = true;
    }
}
