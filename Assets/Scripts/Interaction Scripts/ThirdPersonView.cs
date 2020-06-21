namespace ISAACS {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
        using VRTK;
        using ISAACS_ControllerInput;


    /* TODO
    */

    public class ThirdPersonView : MonoBehaviour
    {
        public GameObject LeftController;
        public GameObject LeftUI;
        public GameObject RightController;
        public GameObject RightUI;

        private VRTK_Pointer LeftPointer;
        private VRTK_BezierPointerRenderer LeftPointerRenderer;
        private VRTK_Pointer RightPointer;
        private VRTK_StraightPointerRenderer RightPointerRenderer;
        private VRTK_UIPointer RightUIPointer;

        public ControllerInput controllerInput; /// Initiate a singleton of the ControllerState class. Assign it to the Controller GameObject.

        private enum ControllerState { IDLE, SCALING, AIMING_SELECTOR, SETTING_WAYPOINT_HEIGHT, PLACING_WAYPOINT, MOVING_WAYPOINT, UNDOING, REDOING }
        private ControllerState controllerState;
        
        public GameObject Pivot; /// pivot is the center of the table
        public GameObject World; /// The World GameObject. All its children will be scaled, rotate and move with it.
        
        private Vector3 WorldScaleInitial; ///originalScale is the original localScale of the world
        private Vector3 WorldScaleMin; /// This is the TODO of the originalScale of the world
        private Vector3 WorldScaleMax; /// This is the TODO times the originalScale of the world

        public float minScale = 0.1f; /// This the minimum size (with respect to the original world size) that the player can scale the world at. For example, MinimumScale = 0.1 signifies that the world can be scaled down to 1/10th of its original size.
        public float maxScale = 10.0f; /// This the maximum size (with respect to the original world size) that the player can scale the world at. For example, MaximumScale = 10 signifies that the world can be scaled up to 10 times its original size.

        public float speed = 1.0f; /// This is the speed at which the map can be moved around.
        public enum Direction { REGULAR, INVERSE }
        public Direction direction = Direction.REGULAR;

        public float rotationalSpeed = 1.0f; /// This is the speed at which the map can be rotated around (revolutions/second).
        public enum RotationalDirection { REGULAR, INVERSE }
        public RotationalDirection rotationalDirection = RotationalDirection.REGULAR;

        //private enum CollisionType { NOTHING, WAYPOINT, OTHER }; // These are the possible values for objects we could be colliding with
        //private CollisionPair mostRecentCollision;
        //private List<CollisionPair> currentCollisions;

        // Use this for initialization
        void Start()
        {
            //This provides us with basis to create bounds on scaling and something to return to
            WorldScaleInitial = World.transform.localScale;

            //These are the bounds on scaling
            WorldScaleMin = Vector3.Scale(WorldScaleInitial, new Vector3(minScale, minScale, minScale));
            WorldScaleMax = Vector3.Scale(WorldScaleInitial, new Vector3(maxScale, maxScale, maxScale));

            LeftPointer = LeftController.GetComponent<VRTK_Pointer>();
            LeftPointerRenderer = LeftController.GetComponent<VRTK_BezierPointerRenderer>();
            RightPointer = RightController.GetComponent<VRTK_Pointer>();
            RightPointerRenderer = RightController.GetComponent<VRTK_StraightPointerRenderer>();
            RightUIPointer = RightController.GetComponent<VRTK_UIPointer>();

            LeftPointer.enabled = false;
            LeftPointerRenderer.enabled = false;
        }


        void FixedUpdate()
        {
            switch(controllerState)
            {
                case ControllerState.IDLE:
                {
                    if (controllerInput.GetBothMiddle())
                    {
                        controllerState = ControllerState.SCALING;
                        LeftUI.SetActive(false);
                        RightUI.SetActive(false);
                        RightPointer.enabled = false;
                        RightPointerRenderer.enabled = false;
                        RightUIPointer.enabled = false;
                        ScaleWorld();
                        break;
                    }

                    if (controllerInput.GetRightMiddle())
                    {
                        controllerState = ControllerState.AIMING_SELECTOR;
                        RightUI.SetActive(false);
                        break;
                    }

                    if (controllerInput.GetRightIndex())
                    {
                        controllerState = ControllerState.PLACING_WAYPOINT;
                        RightPointer.enabled = false;
                        RightPointerRenderer.enabled = false;
                        RightUIPointer.enabled = false;
                        /// TODO: start linecollider
                        break;
                    }

                    if (controllerInput.GetRightA())
                    {
                        controllerState = ControllerState.UNDOING;
                        RightPointer.enabled = false;
                        RightPointerRenderer.enabled = false;
                        RightUIPointer.enabled = false;
                        /// TODO: make waypoint slightly fade
                        break;
                    }

                    if (controllerInput.GetRightB())
                    {
                        controllerState = ControllerState.REDOING;
                        RightPointer.enabled = false;
                        RightPointerRenderer.enabled = false;
                        RightUIPointer.enabled = false;
                        /// TODO: make waypoint slightly appear
                        break;
                    }

                    if (controllerInput.GetRightThumbMoved()) /// Rotate the world
                    {
                        RotateWorld();
                    }

                    if (controllerInput.GetLeftThumbMoved()) /// Move the world
                    {
                        MoveWorld();
                    }

                    break;
                }

                case ControllerState.SCALING:
                {
                    if (controllerInput.GetBothMiddle())
                    {
                        ScaleWorld();
                    }
                    else {
                        controllerState = ControllerState.IDLE;
                        LeftUI.SetActive(true);
                        RightUI.SetActive(true);
                        RightPointer.enabled = true;
                        RightPointerRenderer.enabled = true;
                    }
                    break;
                }

                case ControllerState.AIMING_SELECTOR:
                {
                    /// TODO: set wp height if GetRightIndex()
                    if (controllerInput.GetBothMiddle())
                    {
                        controllerState = ControllerState.SCALING;
                        LeftUI.SetActive(false);
                        RightPointer.enabled = false;
                        RightPointerRenderer.enabled = false;
                        RightUIPointer.enabled = false;
                        ScaleWorld();
                        break;
                    }
                    if (!controllerInput.GetRightMiddle())
                    {
                        controllerState = ControllerState.IDLE;
                        RightUI.SetActive(true);
                    }
                    break;
                }

                case ControllerState.SETTING_WAYPOINT_HEIGHT:
                {
                    break;
                }

                case ControllerState.PLACING_WAYPOINT:
                {
                    if (controllerInput.GetRightMiddle()) /// Cancel waypoint placement
                    {
                        /// TODO: stop showing line
                        controllerState = ControllerState.IDLE;
                        RightPointer.enabled = true;
                        RightPointerRenderer.enabled = true;
                        break;
                    }

                    if (!controllerInput.GetRightIndex())
                    {
                        controllerState = ControllerState.IDLE;
                        CreateWaypoint(RightUI.transform.position);
                        RightPointer.enabled = true;
                        RightPointerRenderer.enabled = true;
                        RightUIPointer.enabled = true;
                    }
                    else
                    {
                        /// TODO: continue line showing
                    }
                    break;

                }

                case ControllerState.MOVING_WAYPOINT:
                {
                    break;
                }

                case ControllerState.UNDOING:
                {
                    if (!controllerInput.GetRightA())
                    {
                        Undo();
                        break;
                    }
                    if (controllerInput.GetRightMiddle())
                    {
                        controllerState = ControllerState.IDLE;
                        RightPointer.enabled = true;
                        RightPointerRenderer.enabled = true;
                        RightUIPointer.enabled = true;
                        /// TODO: make waypoint disappear
                        break;
                    }
                    break;
                }

                case ControllerState.REDOING:
                {
                    if (!controllerInput.GetRightB())
                    {
                        Redo();
                    }
                    if (controllerInput.GetRightMiddle())
                    {
                        controllerState = ControllerState.IDLE;
                        RightPointer.enabled = true;
                        RightPointerRenderer.enabled = true;
                        RightUIPointer.enabled = true;
                        /// TODO: make waypoint disappear
                        break;
                    }
                    break;
                }

            }

        }


        private void ScaleWorld()
        {
            // Get the scaling factor, and adjust its size.
            float ScalingFactor = 1.0f + 0.2f * controllerInput.GetScalingFactor();
            Vector3 ScalingVector = Vector3.Scale(World.transform.localScale, new Vector3(ScalingFactor, ScalingFactor, ScalingFactor));

            //Checking Scaling Bounds
            if (ScalingVector.sqrMagnitude > WorldScaleMin.sqrMagnitude && ScalingVector.sqrMagnitude < WorldScaleMax.sqrMagnitude)
            {
                // FIXME: Jank. and comments.
                Vector3 A = World.transform.position;
                Vector3 B = Pivot.transform.position;
                B.y = A.y;

                Vector3 startScale = World.transform.localScale;
                Vector3 endScale = World.transform.localScale * ScalingFactor;

                Vector3 C = A - B; // diff from object pivot to desired pivot/origin

                // calc final position post-scale
                Vector3 FinalPosition = (C * ScalingFactor) + B;

                // finally, actually perform the scale/translation
                World.transform.localScale = endScale;
                World.transform.position = FinalPosition;
            }

        }


        // Rotate the world based off of the right thumbstick
        private void RotateWorld()
        {
            float angle;
            if (direction == Direction.REGULAR)
            {
                angle = -controllerInput.GetRightThumbDelta().x * rotationalSpeed * 360 * Time.fixedDeltaTime;
            }
            else
            {
                angle = controllerInput.GetRightThumbDelta().x * rotationalSpeed * 360 * Time.fixedDeltaTime;
            }
            World.transform.RotateAround(Pivot.transform.position, Vector3.up, angle);

            // Peru: 5/28/2020 : Point Cloud Rotate
            // FIXME: Is declaring here correct?
            GameObject pointCloud = GameObject.Find("PointCloud");

            if (pointCloud)
            {
                pointCloud.transform.RotateAround(Pivot.transform.position, Vector3.up, angle);
            }
        }


        private void MoveWorld()
        {
            // Negative values are here to make moving around look natural.
            // Without occlusion this looks mediocre because it seems like the map is being moved in the wrong direction.
            float moveX = controllerInput.GetLeftThumbDelta().x;
            float moveZ = controllerInput.GetLeftThumbDelta().y;
            
            // update map position based on input
            Vector3 position = World.transform.position;

            if (direction == Direction.REGULAR)
            {
                position.x -= moveX * speed * Time.deltaTime * 3.0f;
                position.z -= moveZ * speed * Time.deltaTime * 3.0f;
            }
            else
            {
                position.x = moveX * speed * Time.deltaTime * 3.0f;
                position.z = moveZ * speed * Time.deltaTime * 3.0f;
            }


            World.transform.position = position;
        }


        /// <summary>
        /// Instantiates and returns a new waypoint at the placePoint position.
        /// Modifies behavior to add or insert if we are currently colliding with a line
        /// </summary>
        /// <param name="groundPoint"> This is the location on the ground that the waypoint will be directly above. </param>
        /// <returns></returns>
        private Waypoint CreateWaypoint(Vector3 groundPoint)
        {
            Drone currentlySelectedDrone = WorldProperties.GetSelectedDrone(); // Grabbing the drone that we are creating this waypoint for
            // Make sure our drone exists
            if (currentlySelectedDrone != null)
            {
                // We will use the placePoint location.
                Vector3 newLocation = new Vector3(groundPoint.x, groundPoint.y, groundPoint.z);
                // If we don't have a line selected, we default to placing the new waypoint at the end of the path
                Waypoint newWaypoint = new Waypoint(currentlySelectedDrone, newLocation);
                // Add the new waypoint to the drone's path
                currentlySelectedDrone.AddWaypoint(newWaypoint);
                // Return the waypoint to announce that we successfully created one
                return newWaypoint;
            }
            // If we have not added or inserted a waypoint, we need to return null
            return null;
        }

        
        /// <summary>
        /// This method handles the undo and delete functionality
        /// Removes the waypoint from the scene and from the drone's path
        /// </summary>
        public void Undo() /// TODO: add undo for moving around
        {
            Drone currentlySelectedDrone = WorldProperties.GetSelectedDrone();

            if (currentlySelectedDrone != null) {
                // Make sure the currently selected drone has waypoints
                if (currentlySelectedDrone.GetWaypointsCount() >= 1)
                {
                    // Otherwise we default to removing the last waypoint (UNDO)
                    Debug.Log("Removing most recently placed waypoint");

                    Waypoint lastWaypoint = currentlySelectedDrone.PopWaypoint();

                    // Remove from collisions list
                    //currentCollisions.RemoveAll(collision => collision.waypoint == lastWaypoint &&
                     //                       collision.type == CollisionType.WAYPOINT);
                   // currentCollisions.RemoveAll(collision => collision.waypoint == lastWaypoint &&
                     //                       collision.type == CollisionType.LINE);

                    // Catching edge case in which most recent collision was the last waypoint
                    //if (lastWaypoint == mostRecentCollision.waypoint)
                    //{
                     //   mostRecentCollision.type = CollisionType.NOTHING;
                      //  mostRecentCollision.waypoint = null;
                   // }
                }   
            }


        }

        public void Redo()
        {
            Drone currentlySelectedDrone = WorldProperties.GetSelectedDrone();
            if (currentlySelectedDrone != null)
            {
                currentlySelectedDrone.RestoreLastlyDeletedWaypoint();
            }
        }



    }
}