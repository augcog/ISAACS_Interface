using System;
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

    // The current rotation in Euler angles (roll, yaw, pitch)
    private Vector3 angularPosition;
    // The angular velocity vector
    private Vector3 angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
    // The angular acceleration vector
    private Vector3 angularAcceleration = new Vector3(0.0f, 0.0f, 0.0f);

    // The total mass of the quadrotor.
    private float totalMass;

    // TODO The inertia components of the UAV
    private Vector3 inertia; 

    // TODO The speed of each rotor, in the following order:
    private Vector4 rotorForces;

    // Torques. TODO: documentation
    private Vector4 torques;
    private Vector3 torquesOnly;
    private Vector3 targetVelocity;
    private Vector3 direction;

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


    [Header("Simulation Dynamics")]
	[Tooltip("TODO")]
    public Vector3 gravitationalAcceleration = new Vector3(0.0f, 9.81f, 0.0f);
	[Tooltip("TODO")]
    public Vector3 windDisturbance = new Vector3(0.0f, 0.0f, 0.0f);

    [Header("Control Parameters")]
	[Tooltip("TODO")]
    public float targetSpeed = 0.1f; // Desired speed
    public float maximumAccelerationMagnitude = 0.1f;
    public float decelerationDistance = 1.0f;




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

        // velocity = targetSpeed * transform.up;
    }




    // FixedUpdate is called according to the physics engine
    void Update()
    {
        position = transform.localPosition;
        angularPosition = transform.localEulerAngles;
        // angularPosition = transform.up;

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

                // Debug.Log("========= DESTINATION ========== x:" + destination.x + "y" + destination.y + "z" + destination.z);
                rotorForces = QuadrotorDynamics.TargetRotorForces(targetSpeed, destination, position,
                                                                  velocity, angularVelocity, totalMass, inertia,
                                                                  rodLength, dragFactor, thrustFactor, yawFactor,
                                                                  gravitationalAcceleration);
                Debug.Log("*****ROTOR_FORCES****** w: " + rotorForces.w + " x: " + rotorForces.x + " y: " + rotorForces.y + " z: " + rotorForces.z);

                torques = QuadrotorDynamics.SpinRotors(rotorForces, rodLength, dragFactor, thrustFactor, yawFactor);
                torquesOnly.x = torques.x;
                torquesOnly.y = torques.y;
                torquesOnly.z = torques.z;

                angularAcceleration = QuadrotorDynamics.AngularAcceleration(torquesOnly, inertia);
                    Debug.Log("====== Angular Acceleration x:" + angularAcceleration.x + " y: " + angularAcceleration.y + " z: " + angularAcceleration.z);

                angularPosition += angularAcceleration;
                transform.localEulerAngles = angularPosition;
                targetVelocity = targetSpeed * transform.up;

                // angularAcceleration *= Mathf.PI / 180.0f;

				// float sx = Mathf.Sin(angularAcceleration.x / 2.0f);
				// float cx = Mathf.Cos(angularAcceleration.x / 2.0f);
				// float sy = Mathf.Sin(angularAcceleration.y / 2.0f);
				// float cy = Mathf.Cos(angularAcceleration.y / 2.0f);
				// float sz = Mathf.Sin(angularAcceleration.z / 2.0f);
				// float cz = Mathf.Cos(angularAcceleration.z / 2.0f);


                // float angle = 180.0f / Mathf.PI * 2.0f * Mathf.Acos(cx * cy * cz - sx * sy * sz); // may need conversion
                // Vector3 axis; 
                // axis.x = sx * sy * cz + cx * cy * sz;
                // axis.y = sx * cy * cz + cx * sy * sz;
                // axis.z = cz * sy * cz - sx * cy * sz;

                // Vector3 dir = (destination - position).normalized;
                // Vector3.RotateTowards(angularPosition, dir, Mathf.PI * angle / 180.0f);

                // Quaternion rotation = Quaternion.AxisAngle(axis, angle);
                // angularPosition += rotation.eulerAngles;
                // transform.localEulerAngles = angularPosition;

                // transform.up = angularPosition; 


                // Vector3 Up;
                // Compute the direction vector from the given angles.
                // Vector3 upx = new Vector3(sin_uz * sin_ux * sin_uy + cos_uz * cos_uy,      cos_uz * sin_uy * sin_ux - cos_uy * sin_uz      ,    cos_ux * sin_uy);
                // Vector3 upy = new Vector3(sin_uz * cos_ux                           ,      cos_uz * cos_ux   ,     -sin_ux);
                // Vector3 upz = new Vector3(sin_uz * sin_ux * cos_uy - cos_uz * sin_uy,      cos_uz * cos_uy * sin_ux + sin_uz * sin_uy      ,     cos_ux * cos_uy);
                
                // targetVelocity.x = velocity.x * up.x + velocity.x * upx.x + velocity.x * upz.x;
                // targetVelocity.y = velocity.y * up.y + velocity.y * upx.y + velocity.y * upz.y;
                // targetVelocity.z = velocity.z * up.z + velocity.z * upx.z + velocity.z * upz.z;
                // targetVelocity.x = Vector3.Dot(velocity, upx);
                // targetVelocity.y = Vector3.Dot(velocity, upy);
                // targetVelocity.z = Vector3.Dot(velocity, upz);

                // transform.up = targetVelocity.normalized;
                // targetVelocity = targetVelocity.normalized * targetSpeed;

                // targetVelocity =  targetSpeed * (destination - position).normalized;//targetSpeed * transform.up;
                // targetVelocity = targetSpeed * transform.up;
                    // Debug.Log("&&&&&& targetVelocity: " + targetVelocity);
                    // Debug.Log("&&&&&& Velocity BEFORE: " + velocity);

                // angularPosition += angularAcceleration;
                // transform.localEulerAngles = angularPosition;

                // targetVelocity = targetSpeed * transform.up;
                // targetVelocity = velocity + angularAcceleration; 

                // direction = (targetVelocity - velocity - gravitationalAcceleration).normalized;
                    // Debug.Log("&&&&&& DIRECTION: " + direction);
                // direction = angularAcceleration.normalized;
                // targetVelocity = targetSpeed * (destination - position).normalized;
                direction = (targetVelocity - velocity).normalized; 

                // direction = angularAcceleration;

                acceleration = QuadrotorDynamics.Acceleration(torques.w, totalMass, direction, gravitationalAcceleration);
                    Debug.Log("====== Acceleration x:" + acceleration.x + " y: " + acceleration.y + " z: " + acceleration.z);

                velocity += acceleration; //QuadrotorDynamics.Rotation(velocity_body, angular_position);
                    Debug.Log("====== Velocity AFTER: " + velocity);
                // angular_velocity += angular_acceleration; //QuadrotorDynamics.InverseJacobian(angular_velocity_body, angular_position);
                    // Debug.Log("====== Angular Velocity: " + angular_velocity);

                position += velocity;
                    Debug.Log("====== Position: " + position);
                // angular_position += angular_velocity;

                // If the drone "fell undergound", push it back up.
                if (position.y < 0)
                {
                    position.y = 0.0f;
                }
                transform.localPosition = position;

                // angular_position = velocity.normalized;
                // transform.up = angular_position;
                // transform.up = velocity.normalized;

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