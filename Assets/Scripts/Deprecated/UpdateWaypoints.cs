/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ISAACS
{
    public class UpdateWaypoints : MonoBehaviour
    {

        public Material selectedPassedLine;
        public Material unselectedPassedLine;

        public Waypoint targetWaypoint;
        public int currentWaypoint;
        public Waypoint prevPoint;

        private int totalWaypoints;
        private LineRenderer line;
        private Drone thisDrone;

        void Start()
        {
            thisDrone = gameObject.GetComponent<DroneProperties>().droneClassPointer;
            line = this.GetComponentInParent<LineRenderer>();
        }

        void Update()
        {
            if (prevPoint != null)
            {
                DisplayPastPath();
            }

            SelectTarget();
        }

        private void DisplayPastPath()
        {
            line.SetPosition(0, this.transform.position);

            if (thisDrone.waypoints.Count == 0)
            {
                line.SetPosition(1, this.transform.position);
            }
            else
            {
                line.SetPosition(1, prevPoint.gameObjectPointer.transform.position);
            }

            if (thisDrone.selected)
            {
                line.material = selectedPassedLine;
            }
            else
            {
                line.material = unselectedPassedLine;
            }
        }

        private void SelectTarget()
        {
            totalWaypoints = thisDrone.WaypointsCount();
            if (totalWaypoints >= 0)
            {
                if (totalWaypoints > currentWaypoint)
                {
                    targetWaypoint = thisDrone.GetWaypoint(currentWaypoint + 1);
                    if (this.transform.position == targetWaypoint.gameObjectPointer.transform.position)
                    {
                        prevPoint = targetWaypoint;
                        currentWaypoint++;
                    }
                }
                else if (totalWaypoints < currentWaypoint)
                {
                    currentWaypoint = totalWaypoints;
                    prevPoint = thisDrone.GetWaypoint(totalWaypoints);
                }
                else
                {
                    if (targetWaypoint != null && this.transform.position == targetWaypoint.gameObjectPointer.transform.position)
                    {
                        targetWaypoint = null;
                    }
                    else
                    {
                        targetWaypoint = prevPoint;
                    }
                }
            }
        }


    }
}*/