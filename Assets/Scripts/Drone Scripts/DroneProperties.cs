namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DroneProperties : MonoBehaviour {

        // Assigned in Drone constructor
        public Drone classPointer;
        public Material selectedMaterial;
        public Material deselectedMaterial;

        // Assigned in ROS Manager during runtime
        public dynamic droneROSConnection;
        public DroneSimulationManager droneSimulationManager;

    }
}
