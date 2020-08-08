namespace ISAACS {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using VRTK;
    using ISAACS_ControllerInput;


    /** TODO: documentation
    */

    public class ThirdPersonView : MonoBehaviour
    {
        public ControllerInput controllerInput; /// Initiate a singleton of the ControllerState class. Assign it to the Controller GameObject.

        private enum ControllerState { IDLE, SCALING, SELECTING_DRONE, SELECTING_SENSOR, AIMING_SELECTOR, SETTING_WAYPOINT_HEIGHT, PLACING_WAYPOINT, MOVING_WAYPOINT, UNDOING, REDOING }
        private ControllerState controllerState;

        public GameObject World; /// The World GameObject. All its children will be scaled, rotate and move with it.

        private Vector3 WorldScaleInitial; ///originalScale is the original localScale of the world
        private float WorldScaleMin; /// This is the TODO of the originalScale of the world
        private float WorldScaleMax; /// This is the TODO times the originalScale of the world

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


		// Awake is called on initialization, BEFORE Start.
        void Awake()
        {
            // This provides us with basis to create bounds on scaling and something to return to.
            WorldScaleInitial = World.transform.localScale;
            // These are the bounds on scaling.
            WorldScaleMin = Vector3.Scale(WorldScaleInitial, new Vector3(minScale, minScale, minScale)).sqrMagnitude;
            WorldScaleMax = Vector3.Scale(WorldScaleInitial, new Vector3(maxScale, maxScale, maxScale)).sqrMagnitude;
        }


		// Start is called on initialization, AFTER Awake.
        void Start()
        {
            // Enable the required UI elements for this interaction mode.
            controllerInput.EnableRightPointer();
            controllerInput.EnableBothUIs();
        }


        // Fixed update is called n times per frame, where n is the physics step (and can be different from the progression of video frames).
        void FixedUpdate()
        {
            // Query for the controller state, determined by user input.
            switch(controllerState)
            {
                // If nothing is held down, default to the idle state. 
                case ControllerState.IDLE:
                {
                    if (controllerInput.RightIsTouchingWaypoint())
                    {
                        controllerInput.HideWaypointPlacementVisualizer();
                    }
                    else
                    {
                        controllerInput.ShowWaypointPlacementVisualizer();
                    }

                    if (controllerInput.BothGrip())
                    {
                        controllerState = ControllerState.SCALING;

                        controllerInput.DisableBothUIs();
                        controllerInput.DisableRightPointer();

                        ScaleWorld();
                        break;
                    }

                    if (controllerInput.RightGrip())
                    {
                        controllerState = ControllerState.AIMING_SELECTOR;
                        controllerInput.DisableRightUI();
                        break;
                    }

                    if (controllerInput.RightTrigger())
                    {
                        controllerState = ControllerState.PLACING_WAYPOINT;
                        controllerInput.DisableRightPointer();
                        /// TODO: start linecollider
                        break;
                    }

                    if (controllerInput.RightA())
                    {
                        controllerState = ControllerState.UNDOING;
                        controllerInput.DisableRightPointer();
                        /// TODO: make waypoint slightly fade
                        break;
                    }

                    if (controllerInput.RightB())
                    {
                        controllerState = ControllerState.REDOING;
                        controllerInput.DisableRightPointer();
                        /// TODO: make waypoint slightly appear
                        break;
                    }

                    if (controllerInput.RightStickMoved()) /// Rotate the world
                    {
                        RotateWorld();
                    }

                    if (controllerInput.LeftStickMoved()) /// Move the world
                    {
                        MoveWorld();
                    }

                    if (controllerInput.LeftX()) /// Cycle through drones
                    {
                        controllerState = ControllerState.SELECTING_DRONE; 
                    }

                    if (controllerInput.LeftY()) /// Cycle through sensors
                    {
                        controllerState = ControllerState.SELECTING_SENSOR; 
                    }

                    break;
                }

                case ControllerState.SCALING:
                {
                    if (controllerInput.BothGrip())
                    {
                        ScaleWorld();
                    }
                    else {
                        controllerState = ControllerState.IDLE;
                        controllerInput.EnableBothUIs();
                        controllerInput.EnableRightPointer();
                    }
                    break;
                }
                case ControllerState.SELECTING_DRONE:
                {
                    if (!controllerInput.LeftX())
                    {
                        controllerState = ControllerState.IDLE;
                        WorldProperties.SelectNextDrone();
                    }
                    break;
                }

                case ControllerState.SELECTING_SENSOR:
                {
                    if (!controllerInput.LeftY())
                    {
                        controllerState = ControllerState.IDLE;
                        WorldProperties.sensorManager.ShowNextSensor();
                    }
                    break;
                }

                case ControllerState.AIMING_SELECTOR:
                {
                    /// TODO: set wp height if GetRightIndex()
                    if (controllerInput.BothGrip())
                    {
                        controllerState = ControllerState.SCALING;
                        controllerInput.DisableBothUIs();
                        controllerInput.DisableRightPointer();
                        ScaleWorld();
                        break;
                    }
                    if (!controllerInput.RightGrip())
                    {
                        controllerState = ControllerState.IDLE;
                        controllerInput.EnableRightUI();
                    }
                    break;
                }

                case ControllerState.SETTING_WAYPOINT_HEIGHT:
                {
                    break;
                }

                case ControllerState.PLACING_WAYPOINT:
                {
                    if (controllerInput.RightIsGrabbingWaypoint())
                    {
                        controllerState = ControllerState.MOVING_WAYPOINT;
                        break; 
                    }

                    if (controllerInput.RightGrip()) /// Cancel waypoint placement
                    {
                        /// TODO: stop showing line
                        controllerState = ControllerState.IDLE;
                        controllerInput.EnableRightPointer();
                        break;
                    }

                    if (!controllerInput.RightTrigger())
                    {
                        controllerState = ControllerState.IDLE;
                        Drone currentlySelectedDrone = WorldProperties.GetSelectedDrone();
                        currentlySelectedDrone.AddWaypoint(controllerInput.RightUITransform().position);
                        controllerInput.EnableRightPointer();
                    }
                    else
                    {
                        /// TODO: continue line showing and slightly faded wp
                    }
                    break;

                }

                case ControllerState.MOVING_WAYPOINT:
                {
                    if (!controllerInput.RightIsGrabbingWaypoint())
                    {
                        controllerState = ControllerState.IDLE;
                        controllerInput.EnableRightPointer();
                        break;
                    }
                    break;
                }

                case ControllerState.UNDOING:
                {
                    if (!controllerInput.RightA())
                    {
                        controllerState = ControllerState.IDLE;
                        Undo();
                        break;
                    }
                    if (controllerInput.RightGrip())
                    {
                        controllerState = ControllerState.IDLE;
                        controllerInput.EnableRightPointer();
                        /// TODO: make waypoint disappear
                        break;
                    }
                    break;
                }

                case ControllerState.REDOING:
                {
                    if (!controllerInput.RightB())
                    {
                        controllerState = ControllerState.IDLE;
                        Redo();
                        break; 
                    }
                    if (controllerInput.RightGrip())
                    {
                        controllerState = ControllerState.IDLE;
                        controllerInput.EnableRightPointer();
                        /// TODO: make waypoint disappear
                        break;
                    }
                    break;
                }

            }

        }


        private void ScaleWorld()
        {
            // Compute the scaling factor.
            Vector3 velocityDelta = controllerInput.VelocityDelta();            
            Vector3 distance = controllerInput.Distance();            
            float ScalingFactor = 1.0f + 0.2f * Vector3.Dot(velocityDelta, distance);

            // Compute the scaling magnitude.
            Vector3 ScalingVector = Vector3.Scale(World.transform.localScale, new Vector3(ScalingFactor, ScalingFactor, ScalingFactor));
            float scalingMagnitude =  ScalingVector.sqrMagnitude;

            // Check if the scaling magnitude is withing boundaries.
            // Otherwise, the world will become too big or too small. 
            if (scalingMagnitude > WorldScaleMin && scalingMagnitude < WorldScaleMax)
            {
                Vector3 startScale = World.transform.localScale;
                Vector3 endScale = ScalingFactor * startScale;
                // Scaling also moves the object out of place. The following is a temporary fix, but it distorts the map. 
                endScale.y = startScale.y;

                // Finally, scale the world. 
                World.transform.localScale = endScale;
            }
        }


        // Rotate the world based off of the right thumbstick
        private void RotateWorld()
        {
            float angle;
            if (direction == Direction.REGULAR)
            {
                angle = -controllerInput.RightStickDelta().x * rotationalSpeed * 360 * Time.fixedDeltaTime;
            }
            else
            {
                angle = controllerInput.RightStickDelta().x * rotationalSpeed * 360 * Time.fixedDeltaTime;
            }
            World.transform.RotateAround(World.transform.position, Vector3.up, angle);
        }


        private void MoveWorld()
        {
            float moveX = controllerInput.LeftStickDelta().x;
            float moveZ = controllerInput.LeftStickDelta().y;

            // update map position based on input
            Vector3 position = World.transform.position;

            // Get the angle of the headset's rotation
            float theta = controllerInput.HeadsetTransform().rotation.eulerAngles.y;
            // Convert from degrees to radians and get trigonometric constants.
            float theta_sin = (float)Math.Sin(Math.PI / 180.0 * theta);
            float theta_cos = (float)Math.Cos(Math.PI / 180.0 * theta);

            if (direction == Direction.REGULAR)
            {
                // 3.0f is an arbitrary constant for aesthetic purposes.
                // Negative values are here to make moving around look natural.
                // Without occlusion this looks mediocre because it seems like the map is being moved in the wrong direction.
                position.x -= 3.0f * speed * Time.deltaTime * ( theta_cos * moveX + theta_sin * moveZ);
                position.z -= 3.0f * speed * Time.deltaTime * (-theta_sin * moveX + theta_cos * moveZ);
            }
            else
            {
                // 3.0f is an arbitrary constant for aesthetic purposes.
                position.x += 3.0f * speed * Time.deltaTime * ( theta_cos * moveX + theta_sin * moveZ);
                position.z += 3.0f * speed * Time.deltaTime * (-theta_sin * moveX + theta_cos * moveZ);
            }

            World.transform.position = position;
        }

        private void SetWaypointHeight()
        {
            // TODO
        }


        private Waypoint MoveWaypoint()
        {
            //TODO
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
                if (currentlySelectedDrone.WaypointsCount() >= 2)
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
                currentlySelectedDrone.RestoreLastDeletedWaypoint();
            }
        }

    }
}