using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PointCloud;
using ISAACS;

public class PointCloudVisualizer : MonoBehaviour
{
    /// <value> Attach DataServer object. If nonexistant, create an empty GameObject and attach the script `DataServer.cs`.</value>
    public bool flipYZ = true;


    Color red = new Color32(255, 0, 0, 128);
    Color orange = new Color32(255, 130, 0, 128);
    Color yellow = new Color32(255, 255, 0, 128);
    Color green = new Color32(0, 255, 0, 128);
    Color blue = new Color32(0, 0, 255, 128);
    Color light_blue = new Color32(52, 174, 235, 128);
    Color white = new Color32(255, 255, 255, 128);


    /// <summary>
    /// Object to use to represent points in the point cloud.
    /// </summary>
    public GameObject pointObject;

    /// <summary>
    /// scale of the pointObject.
    /// </summary>
    public float size = 0.1f; // see comment below

    private GameObject cloudParent = null;

    private bool hasChanged = false;

    private LampSensor_ROSSensorConnection.PointCloudLevel pointCloudLevel = LampSensor_ROSSensorConnection.PointCloudLevel.WHITE;

    public LampSensor_ROSSensorConnection.PointCloudLevel PointCloudLevel
    {
        get
        {
            return pointCloudLevel;
        }

        set
        {
            pointCloudLevel = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (cloudParent == null)
        {
            cloudParent = new GameObject("Initial");
        }
        pointObject = Resources.Load("RadVoxel") as GameObject;
        // configure the pointObject here.
        //pointObject.transform.localScale = new Vector3(size, size, size);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (hasChanged)
        {
            // Do stuff maybe?
        }
    }

    /// <summary>
    /// Set the parent of this visualizer to the sensor.
    /// </summary>
    /// <param name="parent"></param>
    public void SetParentTransform(Transform parent)
    {
        if (cloudParent == null)
        {
            cloudParent = new GameObject("Initial");
        }
        cloudParent.transform.parent = parent;
    }

    /// <summary>
    /// Replace the current point cloud with a new point cloud.
    /// </summary>
    /// <param name="newCloud"></param>
    public void SetPointCloud(PointCloud<PointXYZRGBAIntensity> newCloud)
    {
        Debug.Log("Setting point cloud XYZ RGBA Intensity");
        Transform parent = cloudParent.transform.parent;
        Destroy(cloudParent);
        // Insert timestamp here maybe?
        cloudParent = new GameObject("PointCloud");
        cloudParent.transform.parent = parent;

        //TODO fixme

        //GameObject world = GameObject.FindGameObjectWithTag("World");
        //cloudParent.transform.parent = world.transform;
        cloudParent.transform.position = new Vector3(0.242f, 2.082f, -0.742f);
        cloudParent.transform.localScale = new Vector3(0.05286196f, 0.05286196f, 0.05286196f);
        //cloudParent.transform.Rotate(0.0f, 163.152f, 0.0f, Space.World);
        cloudParent.transform.Rotate(0.0f, 128.382f, 0.0f, Space.World);

        // END TODO

        bool printOnce = false;
        int cloudLength = newCloud.Points.Count;
        double discard = System.Math.Max(0, 1 - 10000.0 / cloudLength);
        Debug.Log("Discard: " + discard + "\tCount: " + cloudLength);
        foreach (PointXYZRGBAIntensity point in newCloud.Points)
        {

            

            if (Random.value < discard)
            {
                continue;
            }

            

            Vector3 pointPosition = (flipYZ) ? new Vector3(point.X, point.Z, point.Y) : new Vector3(point.X, point.Y, point.Z);
            //Debug.Log("X:" + pointPosition.x + "\tZ:" + pointPosition.z + "\tY" + pointPosition.y);
            
            
            //if (pointPosition.x > 14 || pointPosition.z > -10 || pointPosition.x < -14 || pointPosition.z < -30 || pointPosition.y < -0.03 || pointPosition.y > 5.2)
            //{
            //    continue;
            //}
            
            
            //if(pointPosition - )





            GameObject childPoint = Instantiate(pointObject);
            childPoint.transform.parent = cloudParent.transform;

            childPoint.transform.localPosition = (flipYZ) ? new Vector3(point.X, point.Z, point.Y) : new Vector3(point.X, point.Y, point.Z);
            childPoint.transform.localScale = new Vector3(size, size, size); // size of each point
            //Debug.Log("X:" + childPoint.transform.localPosition.x + "\tZ:" + childPoint.transform.localPosition.z + "\tY" + childPoint.transform.localPosition.y);

            if (!printOnce)
            {
                printOnce = true;
                Debug.Log("R:" + point.R + "\tG:" + point.G + "\tB" + point.B + "\tA" + point.A);
                Debug.Log("Point Cloud Level: " + PointCloudLevel);

            }
            Color color = new Color((float)point.R / 255.0f, (float)point.G / 255.0f, (float)point.B / 255.0f, (float)point.A / 255.0f);
            
            MeshRenderer pRenderer = childPoint.GetComponent<MeshRenderer>();
            Material pMaterial = new Material(Shader.Find("Unlit/Color")); // new material with unlit color

            // Peru: 5/26/20 : New color scheme

            
            if (PointCloudLevel == LampSensor_ROSSensorConnection.PointCloudLevel.RED)
            {
                color = red;
            }
            else if (PointCloudLevel == LampSensor_ROSSensorConnection.PointCloudLevel.ORANGE)
            {
                color = orange;
                
            }
            else if (PointCloudLevel == LampSensor_ROSSensorConnection.PointCloudLevel.YELLOW)
            {
                color = yellow;
            }
            else if (PointCloudLevel == LampSensor_ROSSensorConnection.PointCloudLevel.GREEN)
            {
                color = green;
            }
            else if (PointCloudLevel == LampSensor_ROSSensorConnection.PointCloudLevel.BLUE)
            {
                color = blue;
            }
            else if (PointCloudLevel == LampSensor_ROSSensorConnection.PointCloudLevel.LIGHT_BLUE)
            {
                color = light_blue;
            }
            else
            {
                color = white;
            }


            pMaterial.color = color;
            pRenderer.material = pMaterial;
            // TODO do something with the intensity
        }

        //hasChanged = true;
    }

    /// <summary>
    /// Replace the current point cloud with a new point cloud.
    /// </summary>
    /// <param name="newCloud"></param>
    public void SetPointCloud(PointCloud<PointXYZRGBA> newCloud)
    {
        
        Destroy(cloudParent);
        // Insert timestamp here maybe?
        cloudParent = new GameObject("PointCloud");
        foreach (PointXYZRGBA point in newCloud.Points)
        {
            GameObject childPoint = Instantiate(pointObject);
            childPoint.transform.position = (flipYZ) ? new Vector3(point.X, point.Z, point.Y) : new Vector3(point.X, point.Y, point.Z);
            childPoint.transform.parent = cloudParent.transform;
            
            Color color = new Color(point.R, point.G, point.B, point.A);
            MeshRenderer pRenderer = childPoint.GetComponent<MeshRenderer>();
            Material pMaterial = new Material(Shader.Find("Unlit/Color"));
            pMaterial.color = color;
            pRenderer.material = pMaterial;
        }
        hasChanged = true;
    }


    /// <summary>
    /// Replace the current point cloud with a new point cloud.
    /// </summary>
    /// <param name="newCloud"></param>
    public void SetPointCloud(PointCloud<PointXYZRGB> newCloud)
    {
        Destroy(cloudParent);
        // Insert timestamp here maybe?
        cloudParent = new GameObject("PointCloud");
        foreach (PointXYZRGB point in newCloud.Points)
        {
            GameObject childPoint = Instantiate(pointObject);
            childPoint.transform.position = (flipYZ) ? new Vector3(point.X, point.Z, point.Y) : new Vector3(point.X, point.Y, point.Z);
            childPoint.transform.parent = cloudParent.transform;
            
            Color color = new Color(point.R, point.G, point.B, 1);
            MeshRenderer pRenderer = childPoint.GetComponent<MeshRenderer>();
            Material pMaterial = new Material(Shader.Find("Unlit/Color"));
            pMaterial.color = color;
            pRenderer.material = pMaterial;
        }
        hasChanged = true;
    }


    /// <summary>
    /// Replace the current point cloud with a new point cloud.
    /// </summary>
    /// <param name="newCloud"></param>
    public void SetPointCloud(PointCloud<PointXYZIntensity> newCloud)
    {
        Destroy(cloudParent);
        // Insert timestamp here maybe?
        cloudParent = new GameObject("PointCloud");
        foreach (PointXYZIntensity point in newCloud.Points)
        {
            GameObject childPoint = Instantiate(pointObject);
            childPoint.transform.position = (flipYZ) ? new Vector3(point.X, point.Z, point.Y) : new Vector3(point.X, point.Y, point.Z);
            childPoint.transform.parent = cloudParent.transform;
            // TODO color by intensity
            // Use point.intensity

            MeshRenderer pRenderer = childPoint.GetComponent<MeshRenderer>();
            Material pMaterial = new Material(Shader.Find("Unlit/Color"));
            // TODO: Intensities are porbably between 0 and 1
            Color red = new Color(255, 0, 0, 1);
            Color orange = new Color(255, 130, 0, 1);
            Color yellow = new Color(255, 255, 0, 1);
            Color green = new Color(0, 255, 0, 1);
            Color blue = new Color(0, 0, 255, 1);

            if (point.intensity == 1)
            {
                pMaterial.color = red;
            }
            else if (point.intensity == 0)
            {
                pMaterial.color = orange;
            }
            else if (point.intensity == 0)
            {
                pMaterial.color = yellow;
            }
            else if (point.intensity == 0)
            {
                pMaterial.color = green;
            }
            else if (point.intensity == 0)
            {
                pMaterial.color = blue;
            }
            pRenderer.material = pMaterial;
        }
        hasChanged = true;
    }

}

