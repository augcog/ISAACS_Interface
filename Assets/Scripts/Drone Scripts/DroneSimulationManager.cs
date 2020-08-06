using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

public class DroneSimulationManager : MonoBehaviour {

    [Header("Selected drone and simulation speed")]
    private Drone drone;
    private float speed = 0.1f;

    [Header("Motors")]

	[Tooltip("TODO")]
    public float TorqueConstant;
	[Tooltip("TODO")]
    public float InputCurrent;
	[Tooltip("TODO")]
    public float NoLoadCurrent;
	[Tooltip("TODO")]
    public float BackEMFGeneratedPerRMP;
    // https://en.wikipedia.org/wiki/Motor_constants 
	[Tooltip("TODO")]
    public float MotorResistance;

	[Tooltip("TODO")]
    public float m;
	[Tooltip("TODO")]
    public Vector3 g;
    
	[Tooltip("TODO")]
    public float DragConstant;

    
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

    // TODO: documentation
    private Quaternion currentRotation;



    private int nextWaypointID = 0;


    // Update is called once per frame
    void Update()
    {
        currentLocation = this.transform.localPosition;
        currentRotation = this.transform.localRotation;

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


    // TODO: document
    private Vector3 InertialToBody(Vector3 inertialFrame, Quaternion rotation)
    {
        /* 
        V_B = R(q) * V_I

            -> V_B is the reference frame,
            -> V_I is the intertial frame,
            -> q is tha quaternion (a, b, c, d),
            -> R(q) is the rotation matrix:
        
               | a**2 + b**2 - c**2 - d**2        2*bc - 2*ad               2*bd + 2*ac        |
        R(q) = |        2*bc + 2*ad        a**2 - b**2 + c**2 - d**2        2*cd - 2*ab        |
               |        2*bd - 2*ac               2*cd + 2*ab        a**2 - b**2 - c**2 + d**2 |
        */

        Vector3 bodyFrame;

        float a_2 = rotation.w * rotation.w;
        float b_2 = rotation.x * rotation.x;
        float c_2 = rotation.y * rotation.y;
        float d_2 = rotation.z * rotation.z;

        float ab = rotation.w * rotation.x;
        float ac = rotation.w * rotation.y;
        float ad = rotation.w * rotation.z;
        float bc = rotation.x * rotation.y;
        float bd = rotation.x * rotation.z;
        float cd = rotation.y * rotation.z;

        // Compute R(q)
        Vector3 R1 = new Vector3(a_2 + b_2 - c_2 - d_2, 2 * (bc - ad), 2 * (bd + ac));
        Vector3 R2 = new Vector3(2 * (bc + ad), a_2 - b_2 + c_2 - d_2, 2 * (cd - ab));
        Vector3 R3 = new Vector3(2 * (bd - ac), 2 * (cd + ab), a_2 - b_2 - c_2 + d_2);

        // Compute V_B
        bodyFrame.x = Vector3.Dot(R1, inertialFrame);
        bodyFrame.y = Vector3.Dot(R2, inertialFrame);
        bodyFrame.z = Vector3.Dot(R3, inertialFrame);

        return bodyFrame;
    }

    // TODO: documentation
    private float MotorPower()
    {
        /*
        τ = K_t * (I - I_0)
            -> τ is the Torque
            -> K_t is the Torque Constant
            -> I is the input Current
            -> I_0 is the Current when there is no motor load
        */ 
        float torque = TorqueConstant * (InputCurrent - NoLoadCurrent);
        /*
        P = IV = (τ + K_t * I_0) * (K_t * I_0 * R_m + τ * R_m + K_t * K_v * ω) / K_t**2
            -> P is the motor Power
            -> I is the motor Current
            -> V is the motor Voltage
            -> τ is the Torque
            -> K_t is the Torque Constant
            -> K_v is the Back EMF generated per RPM 
            -> R_m is the motor Resistance
            -> ω is the Angular Velocity
            -> I_0 is the Current when there is no motor load
        */

        float angularVelocity = 1.0f;

        return (torque + TorqueConstant * InputCurrent)
                * (TorqueConstant * InputCurrent * MotorResistance
                   + torque * MotorResistance
                   + TorqueConstant * BackEMFGeneratedPerRMP * angularVelocity)
                / (TorqueConstant * TorqueConstant);  
    }


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
    }

    /// <summary>
    /// Start the drone mission
    /// </summary>
    public void startMission()
    {
        switch (droneStatus)
        {
            case FlightStatus.ON_GROUND_STANDBY:

                if (droneStatusPrev == FlightStatus.NULL)
                {
                    nextWaypointID = 0;

                    foreach(Waypoint waypoint in drone.AllWaypoints())
                    {
                        waypoint.waypointProperties.WaypointUploaded();
                    }

                    updateDestination(true, false, false);

                    droneStatus = FlightStatus.FLYING;
                    droneStatusPrev = FlightStatus.ON_GROUND_STANDBY;

                    drone.droneProperties.StartCheckingFlightProgress(1, drone.WaypointsCount() - 1);
                }
                else
                {
                    if (updateDestination(true, false, false))
                    {
                        droneStatusPrev = droneStatus;
                        droneStatus = FlightStatus.FLYING;
                    }
                    else
                    {
                        Debug.Log("Invalid drone command request, all waypoints completed");
                    }
                }

                break;

            case FlightStatus.IN_AIR_STANDBY:
                if (updateDestination(true, false, false))
                {
                    droneStatusPrev = droneStatus;
                    droneStatus = FlightStatus.FLYING;
                }
                else
                {
                    Debug.Log("Invalid drone command request, all waypoints completed");
                }

                break;

            case FlightStatus.PAUSED_IN_AIR:
                resumeFlight();
                break;

            case FlightStatus.LANDING:
            case FlightStatus.FLYING_HOME:
            case FlightStatus.FLYING:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }
        
    }

    /// <summary>
    /// Pause the flight
    /// </summary>
    public void pauseFlight()
    {
        switch (droneStatus)
        {
            case FlightStatus.ON_GROUND_STANDBY:
            case FlightStatus.IN_AIR_STANDBY:
            case FlightStatus.LANDING:
            case FlightStatus.FLYING_HOME:
            case FlightStatus.FLYING:
                droneStatusPrev = droneStatus;
                droneStatus = FlightStatus.PAUSED_IN_AIR;
                break;

            case FlightStatus.PAUSED_IN_AIR:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }
        
    }

    /// <summary>
    /// Resume a paused flight
    /// </summary>
    public void resumeFlight()
    {
        switch (droneStatus)
        {
            case FlightStatus.ON_GROUND_STANDBY:
            case FlightStatus.IN_AIR_STANDBY:

                if (updateDestination(true, false, false))
                {
                    droneStatusPrev = droneStatus;
                    droneStatus = FlightStatus.FLYING;
                }
                else
                {
                    Debug.Log("Invalid drone command request, all waypoints completed");
                }

                break;

            case FlightStatus.PAUSED_IN_AIR:
                droneStatus = droneStatusPrev;
                droneStatusPrev = FlightStatus.PAUSED_IN_AIR;
                break;

            case FlightStatus.FLYING:
            case FlightStatus.FLYING_HOME:
            case FlightStatus.LANDING:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }
    }

    /// <summary>
    /// The drone flies to the home point
    /// </summary>
    public void flyHome()
    {
        switch (droneStatus)
        {
            case FlightStatus.ON_GROUND_STANDBY:
            case FlightStatus.IN_AIR_STANDBY:
            case FlightStatus.LANDING:
                updateDestination(false, true, false);
                droneStatusPrev = droneStatus;
                droneStatus = FlightStatus.FLYING_HOME;
                break;

            case FlightStatus.FLYING:
            case FlightStatus.PAUSED_IN_AIR:
                nextWaypointID -= 1;
                updateDestination(false, true, false);
                droneStatusPrev = droneStatus;
                droneStatus = FlightStatus.FLYING_HOME;
                break;

            case FlightStatus.FLYING_HOME:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }
        
    }

    /// <summary>
    /// Land the drone at the current point
    /// </summary>
    public void landDrone()
    {
        switch (droneStatus)
        {
            case FlightStatus.IN_AIR_STANDBY:
            case FlightStatus.FLYING_HOME:
                updateDestination(false, false, true);
                droneStatusPrev = droneStatus;
                droneStatus = FlightStatus.LANDING;
                break;

            case FlightStatus.FLYING:
            case FlightStatus.PAUSED_IN_AIR:
                nextWaypointID -= 1;
                updateDestination(false, false, true);
                droneStatusPrev = droneStatus;
                droneStatus = FlightStatus.LANDING;
                break;

            case FlightStatus.ON_GROUND_STANDBY:
            case FlightStatus.LANDING:
            case FlightStatus.NULL:
                Debug.Log("Invalid drone command request");
                break;
        }
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

        if (nextWaypointID == drone.WaypointsCount())
        {
            return false;
        }

        Waypoint nextDestination = drone.GetWaypoint(nextWaypointID);
        currentDestination = nextDestination.gameObjectPointer.transform.localPosition;
        Debug.Log("Destination set to: " + currentDestination);
        nextWaypointID += 1;

        return true;
    }

}
