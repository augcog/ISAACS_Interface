namespace ISAACS {


    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
        using Mapbox.Unity.Map;
        using Mapbox.Utils;
        using ISAACS_UserInput;


    /*
    */

    public class MapInteractions : MonoBehaviour
    {
        public ControllerState controllerState; /// Initiate a singleton of the ControllerState class. Assign it to the Controller GameObject.
        public GameObject World; /// The World GameObject. All its children will be scaled, rotate and move with it.


        public GameObject pivot; /// pivot is the center of the table
        public Vector3 originalScale; ///originalScale is the original localScale of the world
        // FIXME: Remove
        public Vector3 actualScale; ///actualScale is the relative localScale of the world in comparison to its original localScale
        // FIXME: Remove
        public Vector3 currentScale; ///currentScale is the current localScale
        public Vector3 originalPosition; ///This is the original position of the world
        public Vector3 minScale; ///This is the 1/10th of the originalScale of the world
        public Vector3 maxScale; ///This is the 10 times the originalScale of the world

        public float speed = 3; ///This is the speed at which the map can be moved at
        public float rotSpeed = 1; /// Rotation speed (in rev/s)
        public float tableRadius; /// The radius of the table (assuming the table is circular)
        public float mapRadius; /// The radius of the map (assuming the map is circular)
        public GameObject rotatingTable; /// The circular table

        private float movementAngle;
        public float movementAngleDecay = .95f;

        // Pointer Controller
        // private GameObject controller;
        // private VRTK.VRTK_StraightPointerRenderer pointer;

        // Peru: 6/9/2020 : MapBox now completely in MapInteractions
        public bool droneOutOfBounds = false;

        // Mapbox Interactions
        public GameObject citySim;
        public GameObject rsf_roof;

        public AbstractMap abstractMap;
        private float initZoom_citySim = 21.0f;
        private double initLat_citySim = 37.91532757;
        private double initLong_citySim = -122.33805556;

        public bool citySimActive = false;
        private GameObject citySimParent;
        private Vector3 initPosition_citySim;

        public double currLat_citySim;
        public double currLong_citySim;
        private Vector3 currPosition_citySim;

        private float minZoom_citySim = 0.0f;
        private float maxZoom_citySim = 22.0f;


        // Use this for initialization
        void Start()
        {
            //Pivot assignment
            pivot = GameObject.FindWithTag("Table");

            //This provides us with basis to create bounds on scaling and something to return to
            originalScale = World.transform.localScale;
            originalPosition = World.transform.position;
            actualScale = new Vector3(1, 1, 1);

            //These are the bounds on scaling
            minScale = Vector3.Scale(originalScale, new Vector3(0.1F, 0.1F, 0.1F));
            maxScale = Vector3.Scale(originalScale, new Vector3(10F, 10F, 10F));


            //For accessing StraightPointerRenderer and gradually phase it out
            // controller = GameObject.FindGameObjectWithTag("GameController");
            // pointer = controller.GetComponent<VRTK.VRTK_StraightPointerRenderer>();
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

        // Initilize MapBox
        public void InitializeCityMap()
        {

            // Display a map centered around the current drone position
            // Hardcoded to RFS for now
            // TODO: Connect to selected drone when the architecture upgrade allows
            Vector2d intiLatLong = new Vector2d(initLat_citySim, initLong_citySim);
            abstractMap.Initialize(intiLatLong, (int)initZoom_citySim);

            this.citySimActive = true;
            this.initPosition_citySim = citySim.transform.position;
            this.currLat_citySim = initLat_citySim;
            this.currLong_citySim = initLong_citySim;
            this.currPosition_citySim = citySim.transform.position;

            rsf_roof.SetActive(true);
        }

        // Rotate the world based off of the right thumbstick
        private void ControllerRotateWorld()
        {
            float angle = controllerState.GetRightThumbDelta().x * rotSpeed * 360 * Time.fixedDeltaTime;
            World.transform.RotateAround(pivot.transform.position, Vector3.up, angle);

            // Peru: 3/7/2020 : Map Integration Rotate
            if (citySimActive)
            {
                citySimParent.transform.RotateAround(pivot.transform.position, Vector3.up, angle);
            }

            // Peru: 5/28/2020 : Point Cloud Rotate
            GameObject pointCloud = GameObject.Find("PointCloud");

            if (pointCloud)
            {
                pointCloud.transform.RotateAround(pivot.transform.position, Vector3.up, angle);
            }

            if (rotatingTable)
            {
                rotatingTable.transform.RotateAround(pivot.transform.position, Vector3.up, angle);
            }

        }

        private void MoveWorld()
        {

            float moveX = controllerState.GetLeftThumbDelta().x;
            float moveZ = controllerState.GetLeftThumbDelta().y;
            
            // update map position based on input
            Vector3 position = World.transform.position;

            position.x += moveX * speed * Time.deltaTime;
            position.z += moveZ * speed * Time.deltaTime;

            World.transform.position = position;

            // Peru: 3/7/2020 : Map Integration Move
            // TODO: Fix movement if it's along the circumference of the table
            if (citySimActive && !droneOutOfBounds)
            {
                // TODO: find relation between speed and spped_cityMap 
                // (it'll relate to scale somehow)
                float speed_cityMap = speed *2;

                // Calculate delta X,Z
                float deltaX = moveX * speed_cityMap * Time.deltaTime;
                float deltaZ = moveZ * speed_cityMap * Time.deltaTime;

                // Update unity currPosition
                // TODO: make local based on roatation
                currPosition_citySim.x -= deltaX;
                currPosition_citySim.z += deltaZ;

                // Find new Lat,Long
                // TODO: Well shit
                double newLat_citySim = WorldProperties.UnityXToLat(initLat_citySim, currPosition_citySim.x) ;
                double newLong_citySim = WorldProperties.UnityZToLong(initLong_citySim, initLat_citySim, currPosition_citySim.z) ;

                // Update current position
                currLat_citySim = newLat_citySim;
                currLong_citySim = newLong_citySim;
                    
                Vector2d finalLatLong = new Vector2d(currLat_citySim, currLong_citySim);
                abstractMap.UpdateMap(finalLatLong);
            }
        }

        private void ScaleWorld()
        {
            // Get the scaling factor, and adjust its size.
            float ScalingFactor = 1.0f + 0.2f * controllerState.GetScalingFactor();
            Vector3 ScalingVector = Vector3.Scale(World.transform.localScale, new Vector3(ScalingFactor, ScalingFactor, ScalingFactor));

            //Checking Scaling Bounds
            if (ScalingVector.sqrMagnitude > minScale.sqrMagnitude && ScalingVector.sqrMagnitude < maxScale.sqrMagnitude)
            {
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

                // Peru: 3/7/2020 : Map Integration Scale
                if (citySimActive)
                {
                    // Update Zoom of the cityMap
                    //TODO: Figure out the scaling
                    float currZoom = abstractMap.Zoom;
                    float zoomScale = ScalingFactor; 

                    abstractMap.UpdateMap(currZoom* zoomScale);
                }
            }

            currentScale = World.transform.localScale;
            // FIXME. Janky: should use a setter instead. Is it needed?
            WorldProperties.currentScale = currentScale;

            actualScale.x = (currentScale.x / originalScale.x);
            actualScale.y = (currentScale.y / originalScale.y);
            actualScale.z = (currentScale.z / originalScale.z);
            // FIXME. Janky: should use a setter instead. Is it needed?
            WorldProperties.actualScale = actualScale;
        }

    }
}