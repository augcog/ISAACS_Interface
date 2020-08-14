using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

public class DroneSimulationManager : MonoBehaviour {

    // The drone simulated by this script.
    private Drone drone;

    // The current flight status (whether the drone is flying, waiting, landing, and so on).
    private enum FlightStatus
    {
        ON_GROUND_STANDBY = 1,
        IN_AIR_STANDBY = 2,
        FLYING = 3,
        FLYING_HOME = 4,
        PAUSED_IN_AIR = 5,
        LANDING = 6,
        NULL = 7
    }

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
	[Tooltip("The desired speed to maintain while flying.")]
    public float targetSpeed = 0.1f;
	[Tooltip("The distance from a destination from which the drone must start decelerating before it reaches it. Use it to slow down the drone as it reaches a waypoint, or lands.")]
    public float decelerationDistance = 10.0f;
	[Tooltip("When the drone approaches a waypoint, it will measure its distance from the waypoint against this radius to determine whether it has reached it.")]
    public float destinationConfidenceRadius = 0.5f;


    [Header("Simulation Parameters")]
	[Tooltip("The acceleration due to gravity. On the Earth's surface, it is approximately equal to (0, -9.81, 0).")]
    public Vector3 gravitationalAcceleration = new Vector3(0.0f, 9.81f, 0.0f);
	[Tooltip("A vector representing the direction and magnitude of acceleration due to wind. As a general rule, keep its magnitude smaller than the target speed times the total mass.")]
    public Vector3 wind = new Vector3(0.2f, 0.2f, 0.2f);

    [Header("Drone Properties")]
	[Tooltip("The mass of the sphere modeling the body of the drone.")]
    public float bodyMass = 5.0f;
	[Tooltip("The radius of the sphere modeling the body of the drone.")]
    public float bodyRadius = 10.0f;
	[Tooltip("The mass of each individual rotor attached to the drone.")]
    public float rotorMass = 0.4f;
	[Tooltip("The distance between two opposing rotors. It is assumed that the two rods present on the quadrotor are equal.")]
    public float rodLength = 2.0f;
	[Tooltip("A damping factor for the thrust, representing resistance by the drag.")]
    public float dragFactor = 1.0f;
	[Tooltip("The contribution of each rotor force to the total thrust, as well as its x-torque (\"roll force\") and z-torque (\"pitch force\").")]
    public float thrustFactor = 1.0f;
	[Tooltip("The contribution of each rotor force to the y-torque (\"yaw force\").")]
    public float yawFactor = 1.0f;




    /// <summary>
    /// Initilize the drone sim with the required references.
    /// </summary>
    /// <param name="droneInit"></param>
    public void InitDroneSim()
    {
        // Load the drone simulated by this script.
        drone = GetComponent<DroneProperties>().droneClassPointer;

        // Compute the total mass of the quadrotor.
        totalMass = bodyMass + 4 * rotorMass;

        // Model the quadrotor's body and its rotors as spheres, and compute its moments of inertia.
        float bodyInertia = 2.0f * bodyMass * bodyRadius * bodyRadius / 5.0f;
        float rotorInertia = rodLength * rodLength * rotorMass;
        inertia.x = bodyInertia + 2.0f * rotorInertia;
        inertia.y = bodyInertia + 4.0f * rotorInertia;
        inertia.z = bodyInertia + 2.0f * rotorInertia;

        // Store the quadrotor's staring position.
        homeLocation = drone.gameObjectPointer.transform.localPosition;

        // Initialize the flight status.
        droneStatus = FlightStatus.ON_GROUND_STANDBY;
        droneStatusPrev = FlightStatus.NULL;
    }




    // Update is called once per frame.
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

                // Compute the rotor forces needed to fly the drone to the next waypoint at the given target speed.
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

                // Compute the thrust and torques resulting from the above rotor forces.
                thrustForces = QuadrotorDynamics.SpinRotors(rotorForces, rodLength, dragFactor, thrustFactor, yawFactor);

                // Compute the angular acceleration using the torques, and update the angular velocity and angular position of the drone, using Euler's method.
                angularAcceleration = QuadrotorDynamics.AngularAcceleration(thrustForces, inertia);
                angularVelocity += angularAcceleration;
                angularPosition += angularVelocity;
                transform.localEulerAngles = angularPosition;

                // As the angular position has been adjusted, it is possible to compute the linear acceleration.
                acceleration = QuadrotorDynamics.Acceleration(transform.up, thrustForces.w, totalMass, gravitationalAcceleration);
                // Readjust the angular position to fit the direction of the linear acceleration that was computed.
                // This should not make a tremendous difference, but helps stabilize the system.
                transform.up = (acceleration - gravitationalAcceleration).normalized;
                // Add the acceleration due to wind to the linear acceleration.
                acceleration += QuadrotorDynamics.WindDisturbance(wind, transform.up, totalMass);

                // Update the linear velocity and position of the drone, using Euler's method.
                // Although chaotic, at 90 FPS the system should be extremely stable.
                velocity += acceleration;
                position += velocity;

                // Finally, if the drone "fell undergound", push it back up (ground collision).
                if (position.y < 0.0f)
                {
                    position.y = 0.0f;
                }
                // And... this is where the visual magic happens.
                transform.localPosition = position;

                break;

            case FlightStatus.FLYING_HOME:

                // TODO, next: make landing also a function in QuadrotorDynamics, and adjust according to the deceleration distance. Also, the landing may be at an angle: check the ground first.
                // Readjust the quadrotor's angular position: since it's landing, it must be facing upwards.
                transform.up = Vector3.up;
                transform.Translate(0.02f * targetSpeed * (destination - position).normalized, Space.Self);

                if (reachedCurrentDestination())
                {
                    Debug.Log("Drone reached home");
                    droneStatusPrev = FlightStatus.FLYING_HOME;
                    droneStatus = FlightStatus.ON_GROUND_STANDBY;
                }

                break;

            case FlightStatus.LANDING:

                // TODO, next: make landing also a function in QuadrotorDynamics, and adjust according to the deceleration distance. Also, the landing may be at an angle: check the ground first.
                // Readjust the quadrotor's angular position: since it's landing, it must be facing upwards.
                transform.up = Vector3.up;
                transform.Translate(0.02f * targetSpeed * (destination - position).normalized, Space.Self);

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
                nextWaypointID--;
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
    /// Land the drone at the current point.
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
                nextWaypointID--;
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
    /// Check if the drone has reached the current destination.
    /// </summary>
    /// <returns></returns>
    private bool reachedCurrentDestination()
    {
        if (Vector3.Distance(position, destination) < Mathf.Abs(destinationConfidenceRadius))
        {
            return true;
        }
        return false;
    }




    /// <summary>
    /// Update the destination to the next waypoint if possible.
    /// </summary>
    /// <returns>True if updated, false if no more waypoints available.</returns>
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
        nextWaypointID++;

        return true;
    }

}