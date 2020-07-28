namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using VRTK;


    /// <summary>
    /// This class handles the rendering of a waypoint's groundpoint and path line
    /// </summary>
    public class WaypointProperties : MonoBehaviour
    {
        private GameObject controller; // TODO: hardcoded, remove.

        [Header("Connected gameobjects")]
        public Waypoint classPointer;
        public Drone referenceDrone;
        public GameObject referenceDroneGameObject;
        private GameObject prevPoint;

        [Header ("Waypoint Materials")]
        public Material unpassedWaypoint;
        public Material passedWaypoint;
        public Material touchedWaypoint;
        public Material lockedWaypoint;
        public Material uploadedWaypoint;

        [Header("Line Render Materials")]
        public Material selectedUnpassedLine;
        public Material unselectedUnpassedLine;
        public Material selectedPassedLine;
        public Material unselectedPassedLine;
        public Material selectedGroundpointLine;
        public Material unselectedGroundpointLine;

        public enum WaypointStatus
        {
            STATIC = 0, // Indicates if this waypoint is just static, placed and has not been passed.
            PASSED = 1, // Indicates whether this waypoint has been passed by the drone
            GRABBED = 2, // Indicates whether this waypoint is currently grabbed
            LOCKED = 3, // Indicated whether this waypoint is locked (cannot be edited by the user)
            UPLOADED = 4 // Indicated whether this waypoint has been uploaded to the drone
        }
        public WaypointStatus waypointStatus;
        public WaypointStatus prevWaypointStatus;

        public GameObject modelGroundpoint; // Refers to the groundpoint object being instantiated
        private GameObject thisGroundpoint; // groundpoint instantiated under current waypoint
        private LineRenderer groundpointLine; // Connects the groundpoint to the waypoint

        public LineRenderer LineProperties;
        private CapsuleCollider lineCollider;

        private GameObject World;
        private Vector3 WorldScaleInitial;
        private Vector3 WorldScaleCurrent;
        private Vector3 WorldScaleActual;

        public static GameObject controller_right;

        void Start()
        {
            controller = GameObject.Find("Controller");
            waypointStatus = WaypointStatus.STATIC;
            prevWaypointStatus = WaypointStatus.STATIC;

            referenceDrone = classPointer.referenceDrone;
            referenceDroneGameObject = referenceDrone.gameObjectPointer;

            World = GameObject.FindGameObjectWithTag("World");
            WorldScaleInitial = World.transform.localScale;
            controller_right = GameObject.Find("controller_right");

            if (classPointer.prevPathPoint != null)
            {
                lineCollider = new GameObject("Line Collider").AddComponent<CapsuleCollider>();
                lineCollider.tag = "Line Collider";
                lineCollider.isTrigger = true;
                lineCollider.radius = 0.1f;
                lineCollider.gameObject.AddComponent<LineProperties>().originWaypoint = classPointer;
                lineCollider.transform.parent = this.gameObject.transform;

                // Establishing the previous point in the path. (Null if it is the drone)
                prevPoint = classPointer.prevPathPoint.gameObjectPointer;

                // Create the collider around the line renderer
                SetLineCollider();
            }
            else
            {
                prevPoint = null;
            }

            // Sets up interaction events
            GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += new InteractableObjectEventHandler(InteractableObjectUngrabbed);
            GetComponent<VRTK_InteractableObject>().InteractableObjectGrabbed += new InteractableObjectEventHandler(Grabbed);

            //Trying to fix first initial waypoint
            Debug.Log("Init first line");
            SetPassedState();

            SetLine();

            UpdateLineCollider();

            if (thisGroundpoint == null)
            {
                CreateGroundpoint();

            }

            CreateWaypointIndicator();

            //if needed will input again;
            //ChangeColor();

            //static coloring
            this.GetComponent<MeshRenderer>().material = unpassedWaypoint;
            if (referenceDrone.selected)
            {
                LineProperties.material = selectedUnpassedLine;
            }
            else
            {
                LineProperties.material = unselectedUnpassedLine;
            }

            UpdateGroundpointLine();
        }

        //this is called when the object is ungrabbed (was here before somehow)
        void InteractableObjectUngrabbed(object sender, VRTK.InteractableObjectEventArgs e)
        {
            Debug.Log("VRTK Waypoint UnGrabber Function called");


            //stop coroutine
            StopCoroutine(updateLine());

            //inform the reference drone that the flight path has been changed.
            waypointStatus = prevWaypointStatus;
            prevWaypointStatus = WaypointStatus.GRABBED;

            // change the color to un grabbed
            this.GetComponent<MeshRenderer>().material = unpassedWaypoint;

            // Trigger UpdateWaypoints call for drone.
            referenceDrone.DronePathUpdated();
        }

        //called if an object is grabbed
        void Grabbed(object sender, InteractableObjectEventArgs e)
        {
            Debug.Log("VRTK Waypoint Grabber Function called");

            // update the waypoint status
            prevWaypointStatus = waypointStatus;
            waypointStatus = WaypointStatus.GRABBED;

            // change the color to grabbed
            this.GetComponent<MeshRenderer>().material = touchedWaypoint;

            // Start updating the line renderer.
            StartCoroutine(updateLine());
        }

        //Coroutine here to update line ONLY if grabbed
        IEnumerator updateLine()
        {
            while (true)
            {
                if (classPointer.prevPathPoint != null)
                {
                    prevPoint = classPointer.prevPathPoint.gameObjectPointer;
                }

                SetLine();

                UpdateLineCollider();

                if (thisGroundpoint == null)
                {
                    CreateGroundpoint();
                }

                CreateWaypointIndicator();

                UpdateGroundpointLine();

                yield return new WaitForEndOfFrame();

            }
            
        }

        /// <summary>
        /// Change the state and color of the waypoint to unselected.
        /// </summary>
        public void UnSelected()
        {
            GetComponent<VRTK_InteractableObject>().isGrabbable = false;

            switch (waypointStatus)
            {
                case WaypointStatus.STATIC:
                    this.GetComponent<MeshRenderer>().material = unpassedWaypoint;
                    Debug.Log(LineProperties);
                    LineProperties.material = unselectedUnpassedLine;
                    break;
                case WaypointStatus.UPLOADED:
                    this.GetComponent<MeshRenderer>().material = uploadedWaypoint;
                    LineProperties.material = unselectedUnpassedLine;
                    break;
                case WaypointStatus.LOCKED:
                    this.GetComponent<MeshRenderer>().material = lockedWaypoint;
                    LineProperties.material = unselectedUnpassedLine;
                    break;
                case WaypointStatus.GRABBED:
                    this.GetComponent<MeshRenderer>().material = touchedWaypoint;
                    LineProperties.material = unselectedUnpassedLine;
                    break;
                case WaypointStatus.PASSED:
                    this.GetComponent<MeshRenderer>().material = passedWaypoint;
                    LineProperties.material = unselectedPassedLine;
                    break;
            }

        }

        /// <summary>
        /// Change the state and color of the waypoint to selected.
        /// </summary>
        public void Selected()
        {
            GetComponent<VRTK_InteractableObject>().isGrabbable = true;

            switch (waypointStatus)
            {
                case WaypointStatus.STATIC:
                    this.GetComponent<MeshRenderer>().material = unpassedWaypoint;
                    LineProperties.material = selectedUnpassedLine;
                    break;
                case WaypointStatus.UPLOADED:
                    this.GetComponent<MeshRenderer>().material = uploadedWaypoint;
                    LineProperties.material = selectedUnpassedLine;
                    break;
                case WaypointStatus.LOCKED:
                    this.GetComponent<MeshRenderer>().material = lockedWaypoint;
                    LineProperties.material = selectedUnpassedLine;
                    break;
                case WaypointStatus.GRABBED:
                    this.GetComponent<MeshRenderer>().material = touchedWaypoint;
                    LineProperties.material = selectedUnpassedLine;
                    break;
                case WaypointStatus.PASSED:
                    this.GetComponent<MeshRenderer>().material = passedWaypoint;
                    LineProperties.material = selectedPassedLine;
                    break;
            }

        }

        /// <summary>
        /// Change the state and color of the waypoint to passed.
        /// </summary>
        public void WaypointPassed()
        {
            prevWaypointStatus = waypointStatus;
            waypointStatus = WaypointStatus.PASSED;

            GetComponent<VRTK_InteractableObject>().isGrabbable = false;

            this.GetComponent<MeshRenderer>().material = passedWaypoint;

            if (referenceDrone.selected)
            {
                LineProperties.material = selectedPassedLine;
            }
            else
            {
                LineProperties.material = unselectedPassedLine;
            }

        }

        /// <summary>
        /// Lock the waypoint so it cannot be edited by the user.
        /// Change the state, color and VRTK grabable state accordingly
        /// </summary>
        public void LockWaypoint()
        {
            if (waypointStatus == WaypointStatus.PASSED)
            {
                Debug.Log("Invalid command. Please check logic.");
            }

            prevWaypointStatus = waypointStatus;
            waypointStatus = WaypointStatus.LOCKED;

            GetComponent<VRTK_InteractableObject>().isGrabbable = false;

            if (referenceDrone.selected)
            {
                LineProperties.material = selectedUnpassedLine;
            }
            else
            {
                LineProperties.material = unselectedUnpassedLine;
            }

        }

        /// <summary>
        /// Unlock the waypoint so it can be edited by the user.
        /// Change the state, color and VRTK grabble state accordingly
        /// </summary>
        public void UnlockWaypoint()
        {
            waypointStatus = prevWaypointStatus;
            prevWaypointStatus = WaypointStatus.STATIC;
            GetComponent<VRTK_InteractableObject>().isGrabbable = true;
            if (referenceDrone.selected)
            {
                LineProperties.material = selectedUnpassedLine;
            }
            else
            {
                LineProperties.material = unselectedUnpassedLine;
            }

        }

        /// <summary>
        /// Change the state and color of the waypoint to indicate that it has been successfully uploaded to the drone.
        /// </summary>
        public void WaypointUploaded()
        {
            if (waypointStatus == WaypointStatus.PASSED)
            {
                Debug.Log("Invalid command. Please check logic.");
            }

            prevWaypointStatus = waypointStatus;
            waypointStatus = WaypointStatus.UPLOADED;

            this.GetComponent<MeshRenderer>().material = uploadedWaypoint;
            if (referenceDrone.selected)
            {
                LineProperties.material = selectedUnpassedLine;
            }
            else
            {
                LineProperties.material = unselectedUnpassedLine;
            }
        }

        private void ComputeWorldScaleActual() {
            // TODO: check if already computed
            WorldScaleCurrent = World.transform.localScale;
            WorldScaleActual.x = WorldScaleCurrent.x / WorldScaleInitial.x;
            WorldScaleActual.y = WorldScaleCurrent.y / WorldScaleInitial.y;
            WorldScaleActual.z = WorldScaleCurrent.z / WorldScaleInitial.z;
        }

        // Positions line between waypoints and drones
        public void SetLine()
        {
            if (prevPoint != null)
            {
                LineProperties.SetPosition(0, this.transform.position);

                Vector3 endpoint;

                //if (referenceDroneGameObject.GetComponent<MoveDrone>().targetWaypoint != this.gameObject || passed)
                //{
                //    endpoint = prevPoint.transform.position;
                //    LineProperties.SetPosition(1, endpoint);
                //} else
                //{
                //    endpoint = referenceDroneGameObject.transform.position;
                //    LineProperties.SetPosition(1, endpoint);
                //}

                endpoint = prevPoint.transform.position;
                LineProperties.SetPosition(1, endpoint);

                // Code in WaypointSystemUpgrade
                ComputeWorldScaleActual();
                LineProperties.startWidth = WorldScaleActual.y / 200;
                LineProperties.endWidth = WorldScaleActual.y / 200;

                // Code in master
                // LineProperties.startWidth = controller.GetComponent<MapInteractions>().actualScale.y / 200;
                // LineProperties.endWidth = controller.GetComponent<MapInteractions>().actualScale.y / 200;
            }
        }
        
        // Places a collider around the waypoint line
        public void SetLineCollider()
        {
            Vector3 endpoint = prevPoint.transform.position;

            ComputeWorldScaleActual();

            lineCollider.transform.parent = LineProperties.transform;
            
            // Code in WaypointSystemUpgrade
            lineCollider.radius = WorldScaleActual.y / 50;

            // Code in Master
            // lineCollider.radius = controller.GetComponent<MapInteractions>().actualScale.y / 50;

            lineCollider.center = Vector3.zero;
            lineCollider.transform.position = (endpoint + this.gameObject.transform.position) / 2;
            lineCollider.direction = 2;
            lineCollider.transform.LookAt(this.gameObject.transform, Vector3.up);
            lineCollider.height = (endpoint - this.transform.position).magnitude;
            lineCollider.transform.parent = World.transform;
        }

        // Places a collider around the waypoint line
        public void UpdateLineCollider()
        {
            Vector3 endpoint = prevPoint.transform.position;
            lineCollider.transform.position = (endpoint + this.gameObject.transform.position) / 2;
            lineCollider.transform.LookAt(this.gameObject.transform, Vector3.up);
            lineCollider.height = (endpoint - this.transform.position).magnitude;
        }

        // Creates the groundpoint under waypoint
        public void CreateGroundpoint()
        {
            if (groundpointLine != null)
                Destroy(groundpointLine);

            Vector3 groundpoint = new Vector3(this.transform.position.x, World.transform.position.y + modelGroundpoint.transform.localScale.y, this.transform.position.z);
            
            ComputeWorldScaleActual();
            
            thisGroundpoint = Instantiate(modelGroundpoint, groundpoint, Quaternion.identity);
            
            // Code in WaypointSystemUpgrade
            thisGroundpoint.transform.localScale = WorldScaleActual / 100;
            thisGroundpoint.transform.parent = World.transform;

            // Code in master
            // thisGroundpoint.transform.localScale = controller.GetComponent<MapInteractions>().actualScale / 100;
            // thisGroundpoint.transform.parent = world.transform;

            groundpointLine = thisGroundpoint.GetComponent<LineRenderer>();
        }

        // Creates a new Waypoint Indicator
        public void CreateWaypointIndicator()
        {
            ComputeWorldScaleActual();

            groundpointLine.SetPosition(0, thisGroundpoint.transform.position);
            groundpointLine.SetPosition(1, this.transform.position);

            // Code in WaypointSystemUpgrade
            groundpointLine.startWidth = WorldScaleActual.y / 400;
            groundpointLine.endWidth = WorldScaleActual.y / 400;

            // Code in master
            // groundpointLine.startWidth = controller.GetComponent<MapInteractions>().actualScale.y / 400;
            // groundpointLine.endWidth = controller.GetComponent<MapInteractions>().actualScale.y / 400;

            if (referenceDrone.selected)
            {
                groundpointLine.material = selectedGroundpointLine;
            } else
            {
                groundpointLine.material = unselectedGroundpointLine;
            }
        }

        //will delete
        // Changes the colors of waypoints and lines based on their selected and passed states
        //public void ChangeColor()
        //{
            
        //    switch (waypointStatus)
        //    {
        //        case WaypointStatus.STATIC:
        //            this.GetComponent<MeshRenderer>().material = unpassedWaypoint;
        //            if (referenceDrone.selected)
        //            {
        //                LineProperties.material = selectedUnpassedLine;
        //            }
        //            else
        //            {
        //                LineProperties.material = unselectedUnpassedLine;
        //            }
        //            break;
        //        case WaypointStatus.PASSED:
        //            this.GetComponent<MeshRenderer>().material = passedWaypoint;
        //            if (referenceDrone.selected)
        //            {
        //                LineProperties.material = selectedPassedLine;
        //            }
        //            else
        //            {
        //                LineProperties.material = unselectedPassedLine;
        //            }
        //            break;
        //        case WaypointStatus.GRABBED:
        //            this.GetComponent<MeshRenderer>().material = touchedWaypoint;
        //            break;
        //        case WaypointStatus.LOCKED:
        //            this.GetComponent<MeshRenderer>().material = lockedWaypoint;
        //            break;
        //        case WaypointStatus.UPLOADED:
        //            this.GetComponent<MeshRenderer>().material = uploadedWaypoint;
        //            break;
        //    }

        //    switch (waypointStatus)
        //    {
        //        case WaypointStatus.STATIC:
        //        case WaypointStatus.GRABBED:
        //        case WaypointStatus.LOCKED:
        //        case WaypointStatus.UPLOADED:

        //            if (referenceDrone.selected)
        //            {
        //                LineProperties.material = selectedUnpassedLine;
        //            }
        //            else
        //            {
        //                LineProperties.material = unselectedUnpassedLine;
        //            }
        //            break;

        //        case WaypointStatus.PASSED:
        //            if (referenceDrone.selected)
        //            {
        //                LineProperties.material = selectedPassedLine;
        //            }
        //            else
        //            {
        //                LineProperties.material = unselectedPassedLine;
        //            }
        //            break;
        //    }

        //}

        // Destroys groundpoint when waypoint is destroyed
        public void OnDestroy()
        {
            Destroy(thisGroundpoint);
        }

        // Sets this waypoint's passed state
        public void SetPassedState()
        {
            if ( waypointStatus != WaypointStatus.PASSED && Vector3.Distance(referenceDroneGameObject.transform.localPosition , this.transform.localPosition) <0.1f) 
            {
                prevWaypointStatus = waypointStatus;
                waypointStatus = WaypointStatus.PASSED;
                GetComponent<VRTK_InteractableObject>().isGrabbable = false;
            }
        }


        public void DeleteLineCollider()
        {
            //Check if we're at the intial waypoint (no line)
            if (this.lineCollider == null)
            {
                return;
            }
            Destroy(this.lineCollider.gameObject);
        }

        //Update groundpoint line 
        public void UpdateGroundpointLine()
        { 
            if (thisGroundpoint == null) {
                return;
            }

            Vector3 groundPointLocation = new Vector3(this.transform.position.x, World.transform.position.y + modelGroundpoint.transform.localScale.y, this.transform.position.z);
            thisGroundpoint.transform.position = groundPointLocation;
            groundpointLine = thisGroundpoint.GetComponent<LineRenderer>(); 
        }
    }
}
