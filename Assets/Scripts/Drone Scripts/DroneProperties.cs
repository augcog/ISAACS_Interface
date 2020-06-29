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
        public ROSDroneConnectionInterface droneROSConnection;
        public DroneSimulationManager droneSimulationManager;

        /// <summary>
        /// Localize all attached sensors to given position and orientation
        /// </summary>
        public void LocalizeSensors(Vector3 home_position, Quaternion home_oritentation)
        {
            foreach (ROSSensorConnectionInterface sensor in classPointer.attachedSensors)
            {
                sensor.SetLocalOrientation(home_oritentation);
                sensor.SetLocalPosition(home_position);
            }
        }

    }
}
