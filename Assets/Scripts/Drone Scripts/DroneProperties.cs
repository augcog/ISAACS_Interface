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

        // Assigned on the drone prefab
        public DroneSimulationManager droneSimulationManager;
        public DroneMenu droneMenu;

        /// <summary>
        /// Select this drone
        /// </summary>
        public void SelectDrone()
        {
            // Changes the color of the drone to indicate that it has been selected
            this.selectedMeshRenderer.material = selectedMaterial;

            this.droneClassPointer.selected = true;

            // Update the sensor manager
            WorldProperties.sensorManager.initializeSensorUI(droneClassPointer.attachedSensors);

            // Select all the waypoints associated with the drone
            foreach (Waypoint waypoint in droneClassPointer.AllWaypoints())
            {
                waypoint.waypointProperties.Selected();
            }

        }

        /// <summary>
        /// Deselect the current drone
        /// </summary>
        public void DeselectDrone()
        {
            // Changes the color of the drone to indicate that it has been deselected
            this.selectedMeshRenderer.material = deselectedMaterial;
            this.droneClassPointer.selected = false;

            // Unselect all the waypoints associated with the drone
            foreach (Waypoint waypoint in droneClassPointer.AllWaypoints())
            {
                waypoint.waypointProperties.UnSelected();
            }


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


        /// <summary>
        /// Check the distance of the drone from the current target waypoint. 
        /// If reached, inform waypoint it has been passed.
        /// If next waypoint is aviable, lock that and continue checking
        /// If mission completed, inform the ROSConnection and end checking coroutine
        /// </summary>
        /// <returns></returns>
        /*
        IEnumerator CheckTargetWaypoint()
        {
            while (true)
            {
                if (reachedCurrentDestination())
                {
                    // Inform current waypoint it has been passed
                    currentWaypointTarget.waypointProperties.WaypointPassed();

                    // Check if mission is complete
                    if (currentWaypointTargetID == uploadedWaypointsCount)
                    {
                        // Inform ROS Connection that current mission is complete
                        // droneProperties.droneROSConnection.UploadedMissionCompleted();

                        // Stop the checking
                        StopCheckingFlightProgress();
                    }
                    else
                    {
                        // Upadate waypoint target and lock next waypoint.
                        currentWaypointTargetID += 1;
                        currentWaypointTarget = waypoints[currentWaypointTargetID];
                        currentWaypointTarget.waypointProperties.LockWaypoint();

                    }

                }

                yield return new WaitForEndOfFrame();

            }

        }
        */
    }
}
