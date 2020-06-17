using ISAACS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceHardcodedWaypoint : MonoBehaviour {
    
    public double waypointLatitude;
    public double waypointLongitude;
    public float waypointAltitude;

    public double initialLat;

    public GameObject hardcodedWaypointSphere;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (DebuggingManager.droneInitialPositionSet)
        {
           Vector3 changePos = new Vector3(
                   ((float) (WorldProperties.LatDiffMeters(DebuggingManager.droneHomeLat, waypointLatitude)) / WorldProperties.GetUnityXtoLatScale()),
                   ((float) (waypointAltitude /*- WorldProperties.droneHomeAlt*/) / WorldProperties.GetUnityYtoAltScale()),
                   ((float) (WorldProperties.LongDiffMeters(DebuggingManager.droneHomeLong, waypointLongitude, waypointLatitude) / WorldProperties.GetUnityZtoLongScale()))
                 );
            initialLat = DebuggingManager.droneHomeLat;
            this.transform.localPosition = changePos;
        }
		
	}
}
