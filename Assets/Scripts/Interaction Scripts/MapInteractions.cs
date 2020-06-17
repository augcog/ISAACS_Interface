namespace ISAACS {


    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
        using ISAACS_UserInput;


    /* TODO
    */

    public class MapInteractions : MonoBehaviour
    {
        public ControllerState controllerState; /// Initiate a singleton of the ControllerState class. Assign it to the Controller GameObject.
        
        private GameObject World; /// The World GameObject. All its children will be scaled, rotate and move with it.
        private Vector3 WorldScaleInitial; ///originalScale is the original localScale of the world
        private Vector3 WorldScaleMin; /// This is the TODO of the originalScale of the world
        private Vector3 WorldScaleMax; /// This is the TODO times the originalScale of the world

        public float MaximumScale = 10.0f; /// This the maximum size (with respect to the original world size) that the player can scale the world at. For example, MaximumScale = 10 signifies that the world can be scaled up to 10 times its original size.
        public float MinimumScale = 0.1f; /// This the minimum size (with respect to the original world size) that the player can scale the world at. For example, MinimumScale = 0.1 signifies that the world can be scaled down to 1/10th of its original size.

        public float Speed = 1.0f; /// This is the speed at which the map can be moved around.
        public float RotationalSpeed = 1.0f; /// This is the speed at which the map can be rotated around (revolutions/second).

        private GameObject pivot; /// pivot is the center of the table



        // Use this for initialization
        void Start()
        {
            World = GameObject.Find("World");

            //Pivot assignment
            pivot = GameObject.FindWithTag("Table");

            //This provides us with basis to create bounds on scaling and something to return to
            WorldScaleInitial = World.transform.localScale;

            //These are the bounds on scaling
            WorldScaleMax = Vector3.Scale(WorldScaleInitial, new Vector3(MaximumScale, MaximumScale, MaximumScale));
            WorldScaleMin = Vector3.Scale(WorldScaleInitial, new Vector3(MinimumScale, MinimumScale, MinimumScale));

        }

        void FixedUpdate()
        {
            if (controllerState.GetBothMiddle())
            {
                // TODO: Hide Left Panel and lasers (ie. integrate toggle UI.)
                // TODO: Deactivate mesh collider of map when in first-person view, as rotating and scaling also moves the player.
                ScaleWorld();
            }
            
            if (controllerState.GetRightThumbMoved())
            {
                ControllerRotateWorld();
            }

            if (controllerState.GetLeftThumbMoved())
            {
                MoveWorld();
            }
        }


        // Rotate the world based off of the right thumbstick
        private void ControllerRotateWorld()
        {
            float angle = controllerState.GetRightThumbDelta().x * RotationalSpeed * 360 * Time.fixedDeltaTime;
            World.transform.RotateAround(pivot.transform.position, Vector3.up, angle);

            // Peru: 5/28/2020 : Point Cloud Rotate
            // FIXME: Is declaring here correct?
            GameObject pointCloud = GameObject.Find("PointCloud");

            if (pointCloud)
            {
                pointCloud.transform.RotateAround(pivot.transform.position, Vector3.up, angle);
            }
        }

        private void MoveWorld()
        {
            // Negative values are here to make moving around look natural.
            // Without occlusion this looks mediocre because it seems like the map is being moved in the wrong direction.
            float moveX = controllerState.GetLeftThumbDelta().x;
            float moveZ = controllerState.GetLeftThumbDelta().y;
            
            // update map position based on input
            Vector3 position = World.transform.position;

            position.x -= moveX * Speed * Time.deltaTime * 3.0f;
            position.z -= moveZ * Speed * Time.deltaTime * 3.0f;

            World.transform.position = position;
        }

        private void ScaleWorld()
        {
            // Get the scaling factor, and adjust its size.
            float ScalingFactor = 1.0f + 0.2f * controllerState.GetScalingFactor();
            Vector3 ScalingVector = Vector3.Scale(World.transform.localScale, new Vector3(ScalingFactor, ScalingFactor, ScalingFactor));

            //Checking Scaling Bounds
            if (ScalingVector.sqrMagnitude > WorldScaleMin.sqrMagnitude && ScalingVector.sqrMagnitude < WorldScaleMax.sqrMagnitude)
            {
                // FIXME: Jank. and comments.
                Vector3 A = World.transform.position;
                Vector3 B = pivot.transform.position;
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

    }
}