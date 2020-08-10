﻿using System;
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

    // TODO: documentation 
    private int nextWaypointID = 0;

    // Drone state variables
    private FlightStatus droneStatus;
    private FlightStatus droneStatusPrev;

    // TODO: documentation
    private Vector3 homeLocation;
    // The desired position vector 
    private Vector3 destination;

    // The current position vector
    private Vector3 position;
    // The linear velocity vector
    private Vector3 velocity = new Vector3(0.0f, 0.0f, 0.0f);
    // The linear acceleration vector
    private Vector3 acceleration = new Vector3(0.0f, 0.0f, 0.0f);

    // The linear velocity vector in the body frame
    private Vector3 velocity_B = new Vector3(0.0f, 0.0f, 0.0f);
    // The linear acceleration vector in the body frame
    private Vector3 acceleration_B = new Vector3(0.0f, 0.0f, 0.0f);

    // The current rotation in Euler angles (roll, yaw, pitch)
    private Vector3 angular_position;
    // The angular velocity vector
    private Vector3 angular_velocity = new Vector3(0.0f, 0.0f, 0.0f);
    // The angular acceleration vector
    private Vector3 angular_acceleration = new Vector3(0.0f, 0.0f, 0.0f);

    // The angular velocity vector in the body frame
    private Vector3 angular_velocity_B = new Vector3(0.0f, 0.0f, 0.0f);
    // The angular acceleration vector in the body frame
    private Vector3 angular_acceleration_B = new Vector3(0.0f, 0.0f, 0.0f);

    // Torques. TODO: documentation
    private float thrust 
    private float torque_x; 
    private float torque_y; 
    private float torque_z; 

    // The total mass of the UAV
    private float mass;

    // The inertia components of the UAV
    private Vector3 Inertia; 

    [Header("Simulation Dynamics")]

	[Tooltip("TODO")]
    public float targetSpeed = 1.0f; // Desired speed

	[Tooltip("TODO")]
    public float bodyMass = 5.0f;
	[Tooltip("TODO")]
    public float bodyRadius = 15.0f;
	[Tooltip("TODO")]
    public float rotorMass = 0.2f;
	[Tooltip("TODO")]
    public float rodLength = 2.0f;
	[Tooltip("TODO")]
    public Vector3 gravitationalAcceleration = new Vector3(0.0f, -9.81f, 0.0f);
	[Tooltip("TODO")]
    public float dragFactor = 1.0f;
	[Tooltip("TODO")]
    public float thrustFactor = 1.0f;
	[Tooltip("TODO")]
    public float yawFactor = 1.0f;


    /// <summary>
    /// Initilize the drone sim with the required references.
    /// </summary>
    /// <param name="droneInit"></param>
    public void InitDroneSim()
    {
        drone = this.GetComponent<DroneProperties>().droneClassPointer;
        
        // The total mass of the UAV
        mass = bodyMass + 4 * rotorMass;

        // Model the UAV body and rotors as spheres, and compute its inertia
        float bodyInertia = 2.0f * bodyMass * bodyRadius * bodyRadius / 5.0f;
        float rotorInertia = rotorDistance * rotorDistance * rotorMass;
        Inertia.x = bodyInertia + 2.0f * rotorInertia;
        Inertia.y = bodyInertia + 4.0f * rotorInertia;
        Inertia.z = bodyInertia + 2.0f * rotorInertia;

        homeLocation = drone.gameObjectPointer.transform.localPosition;
        position = drone.gameObjectPointer.transform.localPosition;

        droneStatus = FlightStatus.ON_GROUND_STANDBY;
        droneStatusPrev = FlightStatus.NULL;
    }

    // FixedUpdate is called according to the physics engine
    void FixedUpdate()
    {
        position = transform.localPosition;
        rotation = transform.localRotation;

        switch (droneStatus)
        {

            case FlightStatus.FLYING:

                if (reachedCurrentDestination())
                {
                    if (updateDestination(true, false, false) == false)
                    {
                        Debug.Log("All waypoints completed");
                        droneStatusPrev = FlightStatus.FLYING; 
                        droneStatus = FlightStatus.IN_AIR_STANDBY;
                    }
                    
                }

                du2w(); // This updates w

                dv = computeAcceleration();
                dw = computeAngularAcceleration();

                w += dw;
                w2du(); // This updates du
                transform.localEulerAngles += du;
                v += dv;
                transform.localPosition += v;

                // this.transform.Translate(Vector3.Normalize(destination - position)* speed * Time.deltaTime, Space.Self);
                break;

            case FlightStatus.FLYING_HOME:

                this.transform.Translate(Vector3.Normalize(destination - position) * speed * Time.deltaTime, Space.Self);

                if (reachedCurrentDestination())
                {
                    Debug.Log("Drone reached home");
                    droneStatusPrev = FlightStatus.FLYING_HOME;
                    droneStatus = FlightStatus.ON_GROUND_STANDBY;
                }

                break;

            case FlightStatus.LANDING:

                this.transform.Translate(Vector3.Normalize(destination - position) * speed * Time.deltaTime, Space.Self);

                if (reachedCurrentDestination())
                {
                    Debug.Log("Drone landed");
                    droneStatusPrev = FlightStatus.LANDING;
                    droneStatus = FlightStatus.ON_GROUND_STANDBY;
                }

                break;
        }
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
        if (Vector3.Distance(position, destination) < 0.1f)
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
            destination = homeLocation;
            Debug.Log("Destination set to: " + destination);
            return true;
        }

        if (land)
        {
            destination = new Vector3(position.x, homeLocation.y, position.z);
            Debug.Log("Destination set to: " + destination);
            return true;
        }

        if (nextWaypointID == drone.WaypointsCount())
        {
            return false;
        }

        Waypoint nextDestination = drone.GetWaypoint(nextWaypointID);
        destination = nextDestination.gameObjectPointer.transform.localPosition;
        Debug.Log("Destination set to: " + destination);
        nextWaypointID += 1;

        return true;
    }

}
