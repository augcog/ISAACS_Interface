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

        // The waypoint ID the drone is currently flying to
        private int currentWaypointTargetID = 0;

        /// The current waypoint the drone is flying to
        private Waypoint currentWaypointTarget;

        // Total waypoints uploaded to the drone.
        private int uploadedWaypointsCount = 0;

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

        public int CurrentWaypointTargetID()
        {
            return currentWaypointTargetID;
        }

        public int TotalUploadedWaypoints()
        {
            return uploadedWaypointsCount;
        }

        /// <summary>
        /// Update the target waypoint and total uploaded and start checking the distance of the drone from the target waypoint to update the waypoint and mission status.
        /// </summary>
        public void StartCheckingFlightProgress(int _currentWaypointTargetID, int _waypointsUploaded)
        {
            currentWaypointTargetID = _currentWaypointTargetID;
            uploadedWaypointsCount += _waypointsUploaded;
            currentWaypointTarget = droneClassPointer.GetWaypoint(currentWaypointTargetID);
            currentWaypointTarget.waypointProperties.LockWaypoint();
            StartCoroutine(CheckTargetWaypoint());
        }

        /// <summary>
        /// Stop checking the status of the drone flight.
        /// </summary>
        public void StopCheckingFlightProgress()
        {
            StopCoroutine(CheckTargetWaypoint());
            droneROSConnection.UploadedMissionCompleted();
            Debug.Log("Mission Completed!");
        }
        
        /// <summary>
        /// Check the distance of the drone from the current target waypoint. 
        /// If reached, inform waypoint it has been passed.
        /// If next waypoint is aviable, lock that and continue checking
        /// If mission completed, inform the ROSConnection and end checking coroutine
        /// </summary>
        /// <returns></returns>
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
                        currentWaypointTarget = droneClassPointer.GetWaypoint(currentWaypointTargetID);
                        currentWaypointTarget.waypointProperties.LockWaypoint();
                    }

                }

                yield return new WaitForEndOfFrame();

            }

        }
        
        /// <summary>
        /// Check if the drone has reached the current destination
        /// </summary>
        /// <returns></returns>
        private bool reachedCurrentDestination()
        {
            Vector3 currentLocation = this.droneClassPointer.gameObjectPointer.transform.localPosition;
            Vector3 currentDestination = currentWaypointTarget.gameObjectPointer.transform.localPosition;

            if (Vector3.Distance(currentLocation, currentDestination) < 0.5f)
            {
                return true;
            }
            Debug.Log(Vector3.Distance(currentLocation, currentDestination));
            return false;
        }

    }
}
