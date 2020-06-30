namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using VRTK;

    public class DroneProperties : MonoBehaviour
    {

        // Assigned in Drone constructor
        public Drone droneClassPointer;
        public Material selectedMaterial;
        public Material deselectedMaterial;

        // TODO: Move drone selected to DroneProperties from Drone
        // public bool selected;

        // Assigned in ROS Manager during runtime
        public ROSDroneConnectionInterface droneROSConnection;
        public DroneSimulationManager droneSimulationManager;

        public void SelectDrone()
        {
            // TODO: Peru, Jasmine: Update SensorUI in this function.
            // Find the Sensor UI Gameobject: can be stored as a variable in world properties.
            // attachedSensors is the list of ROSSensorInterface in this class that can be send acorss.

            // init the sensor with the following:
            // Create a list of sensor UI's based on attachedSensors.
            // For each sensor UI: have the number of buttons/obtions be no. of subscribers & map every button to a subscriber id.
            // On click: call sensor function to switch ros subscriber on/off
            Debug.Log("Drone selected");

            // Changes the color of the drone to indicate that it has been selected
            this.transform.Find("group3/Outline").GetComponent<MeshRenderer>().material = selectedMaterial;

            WorldProperties.UpdateSelectedDrone(droneClassPointer);
            this.droneClassPointer.selected = true;
        }

        public void DeselectDrone()
        {
            Debug.Log("Drone deselected");
            // Changes the color of the drone to indicate that it has been deselected
            this.transform.Find("group3/Outline").GetComponent<MeshRenderer>().material = deselectedMaterial;
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
