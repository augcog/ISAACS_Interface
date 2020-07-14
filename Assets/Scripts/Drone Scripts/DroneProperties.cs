namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class DroneProperties : MonoBehaviour
    {

        // Assigned in Drone constructor
        public Drone droneClassPointer;
        public MeshRenderer selectedMeshRenderer;
        public Material selectedMaterial;
        public Material deselectedMaterial;


        // Assigned in ROS Manager during runtime
        public ROSDroneConnectionInterface droneROSConnection;
        public DroneSimulationManager droneSimulationManager;
        public DroneMenu droneMenu;

        /// <summary>
        /// Select this drone
        /// </summary>
        public void SelectDrone()
        {
            Debug.Log("Drone selected");

            // Changes the color of the drone to indicate that it has been selected
            this.selectedMeshRenderer.material = selectedMaterial;

            WorldProperties.UpdateSelectedDrone(droneClassPointer);
            this.droneClassPointer.selected = true;

            // Update the sensor manager
            WorldProperties.sensorManager.initializeSensorUI(droneClassPointer.attachedSensors);
        }

        /// <summary>
        /// Deselect the current drone
        /// </summary>
        public void DeselectDrone()
        {
            Debug.Log("Drone deselected");
            // Changes the color of the drone to indicate that it has been deselected
            this.selectedMeshRenderer.material = deselectedMaterial;
            this.droneClassPointer.selected = false;
        }

        /// <summary>
        /// Localize all attached sensors to given position and orientation
        /// </summary>
        public void LocalizeSensors(Vector3 home_position, Quaternion home_oritentation)
        {
            foreach (ROSSensorConnectionInterface sensor in droneClassPointer.attachedSensors)
            {
                sensor.SetLocalOrientation(home_oritentation);
                sensor.SetLocalPosition(home_position);
            }
        }

    }
}
