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

    // The current position vector
    private Vector3 position;
    // The desired position vector 
    private Vector3 destination;

    // The quaternion holding the current rotation;
    // use it to rotate and avoid gimbal lock,
    // but do not modify it directly
    private Quaternion rotation;
    // The roll-yaw-pitch vector
    private Vector3 u = new Vector3(0.0f, 0.0f, 0.0f);
    // The roll-yaw-pitch derivatives vector; not the same as w,
    // the angular velocity, but can be used to compute it
    private Vector3 du = new Vector3(0.0f, 0.0f, 0.0f);

    // The linear velocity vector
    private Vector3 v = new Vector3(0.0f, 0.0f, 0.0f);
    // The linear acceleration vector
    private Vector3 dv = new Vector3(0.0f, 0.0f, 0.0f);

    // The angular velocity vector
    private Vector3 w = new Vector3(0.0f, 0.0f, 0.0f);
    // The angular acceleration vector
    private Vector3 dw = new Vector3(0.0f, 0.0f, 0.0f);

    // Torques. TODO: documentation
    private float tx; 
    private float ty; 
    private float tz; 

    // The total mass of the UAV
    private float m;

    // The inertia components of the UAV
    private float Ixx;
    private float Iyy;
    private float Izz;

    [Header("Simulation Dynamics")]

	[Tooltip("TODO")]
    public float TargetSpeed = 1.0f; // Desired speed

	[Tooltip("TODO")]
    public float bodyMass = 5.0f;
	[Tooltip("TODO")]
    public float bodyRadius = 15.0f;
	[Tooltip("TODO")]
    public float rotorMass = 0.2f;
	[Tooltip("TODO")]
    public float rotorDistance = 2.0f;
	[Tooltip("TODO")]
    public Vector3 gravitationalAcceleration = new Vector3(0.0f, -9.81f, 0.0f);
	[Tooltip("TODO")]
    public Vector3 thrustConstant = new Vector3(1.0f, 1.0f, 1.0f);
	[Tooltip("TODO")]
    public Vector3 frictionConstant = new Vector3(1.0f, 1.0f, 1.0f);



    /// <summary>
    /// Initilize the drone sim with the required references.
    /// </summary>
    /// <param name="droneInit"></param>
    public void InitDroneSim()
    {
        drone = this.GetComponent<DroneProperties>().droneClassPointer;
        
        // The total mass of the UAV
        m = bodyMass + 4 * rotorMass;

        // Model the UAV body and rotors as spheres, and compute its inertia
        float bodyInertia = 2.0f * bodyMass * bodyRadius * bodyRadius / 5.0f;
        float rotorInertia = rotorDistance * rotorDistance * rotorMass;
        Ixx = bodyInertia + 2.0f * rotorInertia;
        Iyy = bodyInertia + 2.0f * rotorInertia;
        Izz = bodyInertia + 4.0f * rotorInertia;

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


    // TODO: document
    private void du2w()
    {
        // Where x is the roll, y is the pitch, z is the yaw
        Vector3 w1 = new Vector3(1.0f,  0.0f,                 -(float)Math.Sin(u.y));
        Vector3 w2 = new Vector3(0.0f,  (float)Math.Cos(u.x),  (float)(Math.Cos(u.y) * Math.Sin(u.x)));
        Vector3 w3 = new Vector3(0.0f, -(float)Math.Sin(u.x),  (float)(Math.Cos(u.y) * Math.Cos(u.x)));

        w.x = Vector3.Dot(w1, du);
        w.y = Vector3.Dot(w2, du);
        w.z = Vector3.Dot(w3, du);
    }

    // TODO: document
    private void w2du()
    {
        // Where x is the roll, y is the pitch, z is the yaw
        Vector3 du1 = new Vector3(1.0f,  (float)(Math.Sin(u.x) * Math.Tan(u.y)),  (float)(Math.Cos(u.x) * Math.Tan(u.y)));
        Vector3 du2 = new Vector3(0.0f,  (float)Math.Cos(u.x),                   -(float)Math.Sin(u.y));
        Vector3 du3 = new Vector3(0.0f,  (float)(Math.Sin(u.x) / Math.Cos(u.y)),  (float)(Math.Cos(u.x) / Math.Cos(u.y)));

        du.x = Vector3.Dot(du1, w);
        du.y = Vector3.Dot(du2, w);
        du.z = Vector3.Dot(du3, w);
    }

    // TODO: document
    private Vector3 computeAcceleration()
    {
        // 4 rotors with equal thrust 
        Vector3 thrust;
        thrust.x = 4 * thrustConstant.x * w.x * w.x; 
        thrust.y = 4 * thrustConstant.y * w.y * w.y; 
        thrust.z = 4 * thrustConstant.z * w.z * w.z; 
        Vector3 thrustAcceleration = R(thrust) / m;

        Vector3 friction;
        friction.x = -frictionConstant.x * v.x;
        friction.y = -frictionConstant.y * v.y;
        friction.z = -frictionConstant.z * v.z;
        Vector3 frictionalAcceleration = friction / m;

        return gravitationalAcceleration + thrustAcceleration + frictionalAcceleration;
    }

    // TODO: document
    private Vector3 computeAngularAcceleration()
    {
        // TODO: torques 
        float dw1 = (tx + w.y * w.z * (Iyy - Izz)) / Ixx;
        float dw2 = (ty + w.x * w.z * (Izz - Ixx)) / Iyy;
        float dw3 = (tz + w.x * w.y * (Ixx - Iyy)) / Izz;
        return new Vector3(dw1, dw2, dw3);
    }

    // TODO: document
    private Vector3 R(Vector3 inertialFrame)
    {
        /* 
        V_B = R(q) * V_I

            -> V_B is the object frame,
            -> V_I is the inertial frame,
            -> q is tha quaternion (a, b, c, d),
            -> R(q) is the rotation matrix:
        
               | a**2 + b**2 - c**2 - d**2        2 * (bc - ad)                2 * (bd + ac)         |
        R(q) = |     2 * (bc + ad)            a**2 - b**2 + c**2 - d**2        2 * (cd - ab)         |
               |     2 * (bd - ac)                2 * (cd + ab)            a**2 - b**2 - c**2 + d**2 |
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
