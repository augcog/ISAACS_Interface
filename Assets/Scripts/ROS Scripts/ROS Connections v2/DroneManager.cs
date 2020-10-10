using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

public class DroneManager : ROSManager {
    /// <summary>
    /// Manager for initializing drones and connecting them to the server
    /// </summary>
    
    override void Start()
    {

    }

    private void InstantiateDrone(Transform position, int uniqueID)
    {
        Drone_v2 newDrone = new Drone_v2(position, uniqueID);
        Debug.Log("Drone that was just made " + uniqueID);

        WorldProperties.AddDrone(newDrone);
    }

}
