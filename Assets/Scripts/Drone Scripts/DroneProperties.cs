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

        // TODO: Move drone selected to DroneProperties from Drone
        // public bool selected;

        // Assigned in ROS Manager during runtime
        public ROSDroneConnectionInterface droneROSConnection;
        public DroneSimulationManager droneSimulationManager;

        public void SelectDrone()
        {
            Debug.Log("Drone selected");

            // Changes the color of the drone to indicate that it has been selected
            this.transform.Find("group3/Outline").GetComponent<MeshRenderer>().material = selectedMaterial;
            this.classPointer.selected = true;
        }

        public void DeselectDrone()
        {
            Debug.Log("Drone deselected");
            // Changes the color of the drone to indicate that it has been deselected
            this.transform.Find("group3/Outline").GetComponent<MeshRenderer>().material = deselectedMaterial;
            this.classPointer.selected = false;

        }

    }
}
