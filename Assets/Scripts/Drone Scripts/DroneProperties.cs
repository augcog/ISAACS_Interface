namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class DroneProperties : MonoBehaviour {

        // Assigned in Drone constructor
        public Drone droneClassPointer;
        public Material selectedMaterial;
        public Material deselectedMaterial;

        // TODO: Move drone selected to DroneProperties from Drone
        // public bool selected;

        // Assigned in ROS Manager during runtime
        public ROSDroneConnectionInterface droneROSConnection;
        public DroneSimulationManager droneSimulationManager;

        // Button attached to gameobject
        Button droneButton;


        void Awake()
        {
            droneButton = GetComponent<Button>(); // <-- you get access to the button component here
            droneButton.onClick.AddListener(() => { OnClickEvent(); });  // <-- you assign a method to the button OnClick event here
        }

        void OnClickEvent()
        {
            if (droneClassPointer.selected)
            {
                DeselectDrone();
            }
            else
            {
                SelectDrone();
            }

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
