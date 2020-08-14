﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

public class DroneSimulationManager : MonoBehaviour {

    // The drone simulated by this script.
    private Drone drone;

    // A uniques identifier for each waypoint.
    private int nextWaypointID = 0;

    // Flight status variables.
    private FlightStatus droneStatus;
    private FlightStatus droneStatusPrev;

    // The starting position vector.
    private Vector3 homeLocation;
    // The desired position vector.
    private Vector3 destination;

    // The current position vector.
    private Vector3 position;
    // The linear velocity vector.
    private Vector3 velocity;
    // The linear acceleration vector.
    private Vector3 acceleration;

    // The current rotation in Euler angles (roll, yaw, pitch).
    private Vector3 angularPosition;
    // The angular velocity vector in Euler angles.
    private Vector3 angularVelocity;
    // The angular acceleration vector in Euler angles.
    private Vector3 angularAcceleration;

    // The mass of the quadrotor.
    private float totalMass;

    // The non-zero moments of inertia of the quadrotor (Ixx, Iyy, Izz).
    private Vector3 inertia;

    // The force exerted by each rotor. Refer to QuadrotorDynamics (or the equivalent dynamics script) for the correct order.
    private Vector4 rotorForces;

    // A vector holding the thrust (.w) and torques (.x, .y, .z) acting on the quadrotor.
    private Vector4 thrustForces;

    [Header("Control Parameters")]
	[Tooltip("TODO")]
    public float targetSpeed = 0.1f;
	[Tooltip("TODO")]
    public float decelerationDistance = 10.0f;

    [Header("Simulation Parameters")]
	[Tooltip("TODO")]
    public Vector3 gravitationalAcceleration = new Vector3(0.0f, 9.81f, 0.0f);
	[Tooltip("TODO")]
    public Vector3 wind = new Vector3(0.01f, 0.01f, 0.01f);

    [Header("Drone Properties")]
	[Tooltip("TODO")]
    public float bodyMass = 5.0f;
	[Tooltip("TODO")]
    public float bodyRadius = 15.0f;
	[Tooltip("TODO")]
    public float rotorMass = 0.2f;
	[Tooltip("TODO")]
    public float rodLength = 2.0f;
	[Tooltip("TODO")]
    public float dragFactor = 1.0f;
	[Tooltip("TODO")]
    public float thrustFactor = 1.0f;
	[Tooltip("TODO")]
    public float yawFactor = 1.0f;
	[Tooltip("Whether the drone is flying, landing, or waiting, among other things.")]
    public enum FlightStatus
    {
        ON_GROUND_STANDBY = 1,
        IN_AIR_STANDBY = 2,
        FLYING = 3,
        FLYING_HOME = 4,
        PAUSED_IN_AIR = 5,
        LANDING = 6,
        NULL = 7
    }



    /// <summary>
    /// Initilize the drone sim with the required references.
    /// </summary>
    /// <param name="droneInit"></param>
    public void InitDroneSim()
    {
        // TODO 
        drone = this.GetComponent<DroneProperties>().droneClassPointer;

        // Compute the total mass of the quadrotor.
        totalMass = bodyMass + 4 * rotorMass;

        // TODO Model the quadrotor body and rotors as spheres, and compute its inertia
        float bodyInertia = 2.0f * bodyMass * bodyRadius * bodyRadius / 5.0f;
        float rotorInertia = rodLength * rodLength * rotorMass;
        inertia.x = bodyInertia + 2.0f * rotorInertia;
        inertia.y = bodyInertia + 4.0f * rotorInertia;
        inertia.z = bodyInertia + 2.0f * rotorInertia;

        // TODO Get the quadrotor's staring position.
        homeLocation = drone.gameObjectPointer.transform.localPosition;
        position = drone.gameObjectPointer.transform.localPosition;

        // Initialize the flight status.
        droneStatus = FlightStatus.ON_GROUND_STANDBY;
        droneStatusPrev = FlightStatus.NULL;
    }




    // FixedUpdate is called according to the physics engine
    void Update()
    {
        position = transform.localPosition;
        angularPosition = transform.localEulerAngles;

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

                rotorForces = QuadrotorDynamics.TargetRotorForces(
                                                                  transform.up,
                                                                  destination,
                                                                  position,
                                                                  targetSpeed,
                                                                  velocity,
                                                                  angularVelocity,
                                                                  inertia,
                                                                  totalMass,
                                                                  gravitationalAcceleration,
                                                                  decelerationDistance,
                                                                  rodLength,
                                                                  dragFactor,
                                                                  thrustFactor,
                                                                  yawFactor
                                                                  );

                thrustForces = QuadrotorDynamics.SpinRotors(rotorForces, rodLength, dragFactor, thrustFactor, yawFactor);

                angularAcceleration = QuadrotorDynamics.AngularAcceleration(thrustForces, inertia);

                angularVelocity += angularAcceleration;
                angularPosition += angularVelocity;
                transform.localEulerAngles = angularPosition;

                Vector3 targetVelocity = targetSpeed * (destination - position).normalized;
                Vector3 direction = (targetVelocity - velocity - gravitationalAcceleration).normalized;

                acceleration = QuadrotorDynamics.Acceleration(transform.up, thrustForces.w, totalMass, gravitationalAcceleration);
                // TODO: documentation
                transform.up = (acceleration - gravitationalAcceleration).normalized;
                acceleration += QuadrotorDynamics.WindDisturbance(acceleration, wind);

                velocity += acceleration;
                position += velocity;

                // If the drone "fell undergound", push it back up.
                if (position.y < 0)
                {
                    position.y = 0.0f;
                }
                transform.localPosition = position;

                break;

            case FlightStatus.FLYING_HOME:

                // Readjust the quadrotor's angular position: since it's landing, it must be facing upwards.
                transform.up = Vector3.up;
                transform.Translate(targetSpeed * (destination - position).normalized, Space.Self);

                if (reachedCurrentDestination())
                {
                    Debug.Log("Drone reached home");
                    droneStatusPrev = FlightStatus.FLYING_HOME;
                    droneStatus = FlightStatus.ON_GROUND_STANDBY;
                }

                break;

            case FlightStatus.LANDING:

                // Readjust the quadrotor's angular position: since it's landing, it must be facing upwards.
                transform.up = Vector3.up;
                transform.Translate(targetSpeed * (destination - position).normalized, Space.Self);

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