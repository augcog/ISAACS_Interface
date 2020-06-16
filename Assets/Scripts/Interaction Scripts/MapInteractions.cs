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
        public ControllerState controllerState; /// Initiate a singleton of the ControllerState class.

        public GameObject[] terrain; /// terrain is the textured heightmap
        public GameObject pivot; /// pivot is the center of the table
        public Vector3 originalScale; ///originalScale is the original localScale of the world
        public Vector3 actualScale; ///actualScale is the relative localScale of the world in comparison to its original localScale
        public Vector3 currentScale; ///currentScale is the current localScale
        public Vector3 originalPosition; ///This is the original position of the world
        public Vector3 minScale; ///This is the 1/10th of the originalScale of the world
        public Vector3 maxScale; ///This is the 10 times the originalScale of the world
        public float speed = 3; ///This is the speed at which the map can be moved at
        public float rotSpeed = 1; /// Rotation speed (in rev/s)
        public bool isRotating; /// True if the map is being rotated
        public enum MapState { IDLE, DRAGGING, MOVING, ROTATING, SCALING }; /// This tells us if the map is still moving or being dragged
        public float tableRadius; /// The radius of the table (assuming the table is circular)
        public float mapRadius; /// The radius of the map (assuming the map is circular)
        public GameObject rotatingTable; /// The circular table

        // Rotation stuff
        public LinkedList<float> angles;
        public bool handleHeldTrigger = false;
        public static MapState mapState;
        public OVRInput.Controller currentController;
        private Vector3 oldVec;
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
            //Terrain assignment
            terrain = GameObject.FindGameObjectsWithTag("Ground");

            //Pivot assignment
            pivot = GameObject.FindWithTag("Table");

            //This provides us with basis to create bounds on scaling and something to return to
            originalScale = transform.localScale;
            originalPosition = transform.position;
            actualScale = new Vector3(1, 1, 1);

            //These are the bounds on scaling
            minScale = Vector3.Scale(originalScale, new Vector3(0.1F, 0.1F, 0.1F));
            maxScale = Vector3.Scale(originalScale, new Vector3(10F, 10F, 10F));

            //handle rotation
            mapState = MapState.IDLE;
            angles = new LinkedList<float>();

            //For accessing StraightPointerRenderer and gradually phase it out
            // controller = GameObject.FindGameObjectWithTag("GameController");
            // pointer = controller.GetComponent<VRTK.VRTK_StraightPointerRenderer>();
        }

        void FixedUpdate()
        {
            //if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
            if (controllerState.GetBothMiddle())
            {
                // SCALE WORLD - if both grip triggers are held
                ScaleWorld();
                UpdateScale();
            }
            else
            {
                //POINTER MODE - this method should check for just one grip input and activate the pointer mode.

                // ROTATE WORLD - these methods check for just one grip input on a turntable handle or the right joystick moving
                ControllerRotateWorld();
            }

            // MOVING WORLD
            MoveWorld();
            EnforceMapBoundary();
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
            float deltaX = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

            // We only consider inputs above a certain threshold.
            if (Mathf.Abs(deltaX) > 0.2f)
            {
                mapState = MapState.IDLE; // Controller input overrides manual
                float angle = deltaX * rotSpeed * 360 * Time.fixedDeltaTime;
                transform.RotateAround(pivot.transform.position, Vector3.up, angle);

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
                mapState = MapState.ROTATING;
            }
            else if (mapState == MapState.ROTATING)
            {
                mapState = MapState.IDLE;
            }
        }

        private void MoveWorld()
        {
            float moveX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
            float moveZ = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
            
            if (moveX != 0 || moveZ != 0)
            {
                // update map position based on input
                Vector3 position = transform.position;

                position.x += moveX * speed * Time.deltaTime;
                position.z += moveZ * speed * Time.deltaTime;

                transform.position = position;
                mapState = MapState.MOVING;

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
            else if (mapState == MapState.MOVING)
            {
                mapState = MapState.IDLE;
            }

           
        }

        private void ScaleWorld()
        {
            //Obtaining distance and velocity
            Vector3 d = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch) - OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Vector3 v = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch) - OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);

            //Calculating Scaling Vector
            float result = Vector3.Dot(v, d);

            //Adjusting result to slow scaling
            float final_result = 1.0F + 0.2F * result;

            Vector3 scalingFactor = Vector3.Scale(transform.localScale, new Vector3(final_result, final_result, final_result));

            //Checking Scaling Bounds
            if (scalingFactor.sqrMagnitude > minScale.sqrMagnitude && scalingFactor.sqrMagnitude < maxScale.sqrMagnitude)
            {
                Vector3 A = transform.position;
                Vector3 B = pivot.transform.position;
                B.y = A.y;

                Vector3 startScale = transform.localScale;
                Vector3 endScale = transform.localScale * final_result;

                Vector3 C = A - B; // diff from object pivot to desired pivot/origin

                // calc final position post-scale
                Vector3 FinalPosition = (C * final_result) + B;

                // finally, actually perform the scale/translation
                transform.localScale = endScale;
                transform.position = FinalPosition;
                mapState = MapState.SCALING;

                // Peru: 3/7/2020 : Map Integration Scale
                if (citySimActive)
                {
                    // Update Zoom of the cityMap
                    //TODO: Figure out the scaling
                    float currZoom = abstractMap.Zoom;
                    float zoomScale = final_result; 

                    abstractMap.UpdateMap(currZoom* zoomScale);
                }
            }
            else if (mapState == MapState.SCALING)
            {
                mapState = MapState.IDLE;
            }
        }

        private void UpdateScale()
        {
            currentScale = transform.localScale;
            WorldProperties.currentScale = currentScale;

            actualScale.x = (currentScale.x / originalScale.x);
            actualScale.y = (currentScale.y / originalScale.y);
            actualScale.z = (currentScale.z / originalScale.z);
            WorldProperties.actualScale = actualScale;
        }

        // Makes sure the map sits within the boundaries of the visible table.
        // Checks if two circles (tableCenter w/ radius tableRadius, and mapCenter w/ radius worldMapRadius), intersect
        // If not, then moves the map towards the center until it does.
        private void EnforceMapBoundary()
        {
            Vector3 tableCenter = originalPosition;
            Vector3 mapCenter = transform.position;
            float worldMapRadius = mapRadius * transform.localScale.x;

            // Distance check
            float distSqr = Vector3.SqrMagnitude(tableCenter - mapCenter);
            if (distSqr > Mathf.Pow(tableRadius + worldMapRadius, 2))
            {
                // Peru: 3/7/2020 : Map Integration
                droneOutOfBounds = true;
                // Peru: 3/7/2020 : Map Integration


                // Create vector from mapCenter to edge of table circle (in bounds)
                float distDiff = Vector3.Distance(tableCenter, mapCenter) - tableRadius - worldMapRadius;
                Vector3 movement = Vector3.Normalize(tableCenter - mapCenter) * distDiff;
                movement.y = 0;

                // Move
                transform.Translate(movement, Space.World);
            }
            else
            {
                droneOutOfBounds = false;
            }
        }

    }
}