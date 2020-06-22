using ISAACS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DroneMenu : MonoBehaviour {

    public Text dronePosText;
    public Text droneNameText;
    public Text droneAuthorityText;
    GameObject menuCanvas;
    double homeLat;
    double homeLong;
    bool hasAuthority;

    // Use this for initialization
    void Start () {
        //f = GameObject.Find("DronePos");
        //Debug.Log(f);
        menuCanvas = GameObject.Find("DroneMenuCanvas");
        menuCanvas.transform.SetParent(this.gameObject.transform);
        //float scale = 0.00361908f;
        //menuCanvas.transform.localScale = new Vector3(scale, scale, scale);
        //menuCanvas.transform.localPosition = new Vector3(0.0f, 0.37f, 0.0f);


        dronePosText = GameObject.Find("DronePos").GetComponent<Text>();
        droneNameText = GameObject.Find("DroneName").GetComponent<Text>();
        droneAuthorityText = GameObject.Find("DroneAuthority").GetComponent<Text>();


        // drone -> Matric connection -> get home lat
        homeLat = this.GetComponent<M210_ROSDroneConnection>().GetHomeLat();
        homeLong = this.GetComponent<M210_ROSDroneConnection>().GetHomeLong();
        hasAuthority = this.GetComponent<DroneProperties>().droneROSConnection.HasAuthority();
    }

    // Update is called once per frame
    void Update(){

        GameObject drone = this.gameObject;
        Vector3 dronePosition = drone.transform.position;

        //menuCanvas.transform.LookAt(GameObject.Find("OVRCameraRig").transform); // TODO: make menu always face the user

        //37.915345, -122.337932
        double lat = WorldProperties.UnityXToLat(37.915345, dronePosition.x); // HARDCODED
        double lon = WorldProperties.UnityZToLong(-122.337932, lat, dronePosition.z); // HARDCODED
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
