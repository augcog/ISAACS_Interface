using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ISAACS
{
    public class UpdateWaypoints : MonoBehaviour
    {

        public Material selectedPassedLine;
        public Material unselectedPassedLine;
        public GameObject prevPoint;

        private int totalWaypoints;
        private LineRenderer line;
        private Drone thisDrone;

        void Start()
        {
            thisDrone = gameObject.GetComponent<DroneProperties>().classPointer;
            line = this.GetComponentInParent<LineRenderer>();
        }

        void Update()
        {
            totalWaypoints = thisDrone.waypoints.Count - 1;

            if (prevPoint != null)
            {
                DisplayPastPath();
            }

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
                line.SetPosition(1, prevPoint.transform.position);
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

    }

}


