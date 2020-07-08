using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

public class DroneSimulationManager : MonoBehaviour {

    [Header("Selected drone and simulation speed")]
    public Drone drone;
    public float speed = 1.0f;

    private int nextWaypointID = 0;
    private bool flying = false;

    private float startTime;
    private float journeyLength;
    private Vector3 origin;
    private Vector3 destination;

    private Vector3 home;

    private bool endFlight = false;


    private float fractionOfDistanceCovered = 0;

    // Update is called once per frame
    void Update()
    {
        if (flying)
        {
            if (fractionOfDistanceCovered < 1)
            {
                // Distance moved equals elapsed time times speed..
                float distCovered = (Time.time - startTime) * speed;

                // Fraction of journey completed equals current distance divided by total distance.
                float fractionOfJourney = distCovered / journeyLength;

                Vector3 new_position = Vector3.Lerp(origin, destination, fractionOfJourney);
                drone.gameObjectPointer.transform.localPosition = new_position;
            }
            else
            {
                flying = false;
                if (!endFlight)
                {
                    FlyNextWaypoint();
                }
            }

        }

    }

    public void InitDroneSim(Drone droneInit)
    {
        Debug.Log("Drone Flight Sim initilized");
        drone = droneInit;
        home = drone.gameObjectPointer.transform.localPosition;
        Debug.Log("The selected drone is: " + drone.gameObjectPointer.name);
    }

    public void FlyNextWaypoint(bool restart = false)
    {
        // TODO: debug drone variable osciallting
        List<Waypoint> waypoints = WorldProperties.GetSelectedDrone().waypoints;

        if (restart)
        {
            endFlight = false;
            nextWaypointID = 0;
        }

        /// Check if there is another waypoint
        if (waypoints.Count == nextWaypointID)
        {
            Debug.Log("ALERT: All waypoints successfully send");
            Debug.Log("ALERT: Drone is send home by default");
            flying = false;
            return;
        }

        Waypoint waypoint = (Waypoint)waypoints[nextWaypointID];

        startTime = Time.time;
        origin = drone.gameObjectPointer.transform.localPosition;
        destination = waypoint.gameObjectPointer.transform.localPosition;
        journeyLength = Vector3.Distance(origin, destination);

        flying = true;

        nextWaypointID += 1;
        fractionOfDistanceCovered = 0.0f;

    }

    public void FlyNextWaypoint(Vector3 waypoint)
    {
        origin = drone.gameObjectPointer.transform.localPosition;
        destination = waypoint;

        Debug.Log("Origin: " + origin);
        Debug.Log("Dest:   " + destination);

        flying = true;
    }

    public void pauseFlight()
    {
        flying = false;
    }

    public void resumeFlight()
    {
        flying = true;
    }

    public void flyHome()
    {
        endFlight = true;
        FlyNextWaypoint(home);
    }

}
