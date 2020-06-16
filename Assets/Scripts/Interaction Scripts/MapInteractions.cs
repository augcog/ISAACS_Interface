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
        public GameObject World; /// The World GameObject. All its children will be scaled, rotate and move with it.


        private GameObject pivot; /// pivot is the center of the table

        private Vector3 originalScale; ///originalScale is the original localScale of the world
        // FIXME: Remove.
        public Vector3 actualScale; ///actualScale is the relative localScale of the world in comparison to its original localScale
        // FIXME: Remove.
        private Vector3 currentScale; ///currentScale is the current localScale

        public float MinimumScale; /// Let the user specify TODO
        public float MaximumScale; /// Let the user specify TODO
        private Vector3 minScale; ///This is the TODO of the originalScale of the world
        private Vector3 maxScale; ///This is the TODO times the originalScale of the world

        public float Speed; ///This is the speed at which the map can be moved at
        public float RotationalSpeed; /// Rotation speed (in rev/s)


        // Use this for initialization
        void Start()
        {
            //Pivot assignment
            pivot = GameObject.FindWithTag("Table");

            //This provides us with basis to create bounds on scaling and something to return to
            originalScale = World.transform.localScale;
            actualScale = new Vector3(1, 1, 1);

            //These are the bounds on scaling
            minScale = Vector3.Scale(originalScale, new Vector3(1.0f / MinimumScale, 1.0f / MinimumScale, 1.0f / MinimumScale));
            maxScale = Vector3.Scale(originalScale, new Vector3(MaximumScale, MaximumScale, MaximumScale));

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
            float moveX = -controllerState.GetLeftThumbDelta().x;
            float moveZ = -controllerState.GetLeftThumbDelta().y;
            
            // update map position based on input
            Vector3 position = World.transform.position;

            position.x += moveX * Speed * Time.deltaTime;
            position.z += moveZ * Speed * Time.deltaTime;

            World.transform.position = position;
        }

        private void ScaleWorld()
        {
            // Get the scaling factor, and adjust its size.
            float ScalingFactor = 1.0f + 0.2f * controllerState.GetScalingFactor();
            Vector3 ScalingVector = Vector3.Scale(World.transform.localScale, new Vector3(ScalingFactor, ScalingFactor, ScalingFactor));

            //Checking Scaling Bounds
            if (ScalingVector.sqrMagnitude > minScale.sqrMagnitude && ScalingVector.sqrMagnitude < maxScale.sqrMagnitude)
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


            // FIXME. Janky: should use a setter instead. Or even better, query directly the WorldScale, than keep a useless variable in WOrldProperties.
            currentScale = World.transform.localScale;
            WorldProperties.currentScale = currentScale;
            // FIXME. Janky: should use a setter instead. Or even better, query directly the WorldScale, than keep a useless variable in WOrldProperties.
            actualScale.x = (currentScale.x / originalScale.x);
            actualScale.y = (currentScale.y / originalScale.y);
            actualScale.z = (currentScale.z / originalScale.z);
            WorldProperties.actualScale = actualScale;
        }

    }
}