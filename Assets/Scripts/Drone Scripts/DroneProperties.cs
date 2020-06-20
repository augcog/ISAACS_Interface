namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using VRTK;

    public class DroneProperties : VRTK_InteractableObject
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

        public override void StartUsing(VRTK_InteractUse currentUsingObject = null)
        {
            base.StartUsing(currentUsingObject);
            Debug.Log("Something should start happening?");
        }

        public override void StopUsing(VRTK_InteractUse previousUsingObject = null, bool resetUsingObjectState = true)
        {
            base.StopUsing(previousUsingObject, resetUsingObjectState);
            Debug.Log("Something should stop happening?");
        }

        public void SelectDrone()
        {
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

    }
}
