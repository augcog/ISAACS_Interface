using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

public class DroneSimulationManager : MonoBehaviour {

    [Header("Selected drone and simulation speed")]
    private Drone drone;
    private float speed = 0.1f;

    /// <summary>
    /// Current flight status of the drone
    /// </summary>
    public enum FlightStatus
    {
        ON_GROUND_STANDBY = 1,
        IN_AIR_STANDBY =2,
        FLYING = 3,
        FLYING_HOME = 4,
        PAUSED_IN_AIR = 5,
        LANDING = 6,
        NULL = 7
    }

    // Drone state variables
    private FlightStatus droneStatus;
    private FlightStatus droneStatusPrev;

    private Vector3 currentLocation;
    private Vector3 currentDestination;
    private Vector3 homeLocation;

    List<Waypoint> waypoints;
    private int nextWaypointID = 0;

    /// <summary>
    /// Initilize the drone sim with the required references.
    /// </summary>
    /// <param name="droneInit"></param>
    public void InitDroneSim()
    {
        drone = this.GetComponent<DroneProperties>().droneClassPointer;
  
        homeLocation = drone.gameObjectPointer.transform.localPosition;
        currentLocation = drone.gameObjectPointer.transform.localPosition;

        droneStatus = FlightStatus.ON_GROUND_STANDBY;
        droneStatusPrev = FlightStatus.NULL;

        waypoints = drone.waypoints;
    }

    /// <summary>
    /// Start the drone mission
    /// </summary>
    public void startMission()
    {
        nextWaypointID = 0;
        waypoints = drone.waypoints;
        updateDestination(true, false, false);
        droneStatus = FlightStatus.FLYING;
        droneStatusPrev = FlightStatus.ON_GROUND_STANDBY;
    }

    /// <summary>
    /// Pause the flight
    /// </summary>
    public void pauseFlight()
    {
        droneStatusPrev = droneStatus;
        droneStatus = FlightStatus.PAUSED_IN_AIR;
    }

    /// <summary>
    /// Resume a paused flight
    /// </summary>
    public void resumeFlight()
    {
        droneStatus = droneStatusPrev;
        droneStatusPrev = FlightStatus.PAUSED_IN_AIR;
    }

    /// <summary>
    /// The drone flies to the home point
    /// </summary>
    public void flyHome()
    {
        switch (droneStatus)
        {
            case FlightStatus.FLYING:
                nextWaypointID -= 1;
                break;
        }

        updateDestination(false, true, false);
        droneStatusPrev = droneStatus;
        droneStatus = FlightStatus.FLYING_HOME;
    }

    /// <summary>
    /// Land the drone at the current point
    /// </summary>
    public void landDrone()
    {
        switch (droneStatus)
        {
            case FlightStatus.FLYING:
                nextWaypointID -= 1;
                break;
        }
        
        updateDestination(false, false, true);
        droneStatusPrev = droneStatus;
        droneStatus = FlightStatus.LANDING;
    }

    /// <summary>
    /// update the waypoints list
    /// </summary>
    public void updateWaypoints()
    {
        waypoints = drone.waypoints;
    }

    /// <summary>
    /// Check if the drone has reached the current destination
    /// </summary>
    /// <returns></returns>
    private bool reachedCurrentDestination()
    {
        if (Vector3.Distance(currentLocation, currentDestination) < 0.1f)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Update the destination to the next waypoint if possible
    /// </summary>
    /// <returns>True if updated, false if no more waypoints available</returns>
    private bool updateDestination(bool waypoint, bool home, bool land)
    {

        if (home)
        {
            currentDestination = homeLocation;
            Debug.Log("Destination set to: " + currentDestination);
            return true;
        }

        if (land)
        {
            currentDestination = new Vector3(currentLocation.x, homeLocation.y, currentLocation.z);
            Debug.Log("Destination set to: " + currentDestination);
            return true;
        }

        if (nextWaypointID == waypoints.Count)
        {
            return false;
        }

        Waypoint nextDestination = waypoints[nextWaypointID];
        currentDestination = nextDestination.gameObjectPointer.transform.localPosition;
        Debug.Log("Destination set to: " + currentDestination);
        nextWaypointID += 1;

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        currentLocation = this.transform.localPosition;

        switch (droneStatus)
        {

            case FlightStatus.FLYING:

                this.transform.Translate(Vector3.Normalize(currentDestination-currentLocation)* speed * Time.deltaTime, Space.Self);

                if (reachedCurrentDestination())
                {
                    if (updateDestination(true, false, false) == false)
                    {
                        Debug.Log("All waypoints completed");
                        droneStatusPrev = FlightStatus.FLYING; 
                        droneStatus = FlightStatus.IN_AIR_STANDBY;
                    }
                    
                }

                break;

            case FlightStatus.FLYING_HOME:

                this.transform.Translate(Vector3.Normalize(currentDestination - currentLocation) * speed * Time.deltaTime, Space.Self);

                if (reachedCurrentDestination())
                {
                    Debug.Log("Drone reached home");
                    droneStatusPrev = FlightStatus.FLYING_HOME;
                    droneStatus = FlightStatus.ON_GROUND_STANDBY;
                }

                break;

            case FlightStatus.LANDING:

                this.transform.Translate(Vector3.Normalize(currentDestination - currentLocation) * speed * Time.deltaTime, Space.Self);

                if (reachedCurrentDestination())
                {
                    Debug.Log("Drone landed");
                    droneStatusPrev = FlightStatus.LANDING;
                    droneStatus = FlightStatus.ON_GROUND_STANDBY;
                }

                break;
        }
    }

}
