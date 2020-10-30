using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using ROSBridgeLib.voxblox_msgs;
using ROSBridgeLib.rntools;

using ISAACS;

public class PCFaceSensor_ROSSensorConnection : MonoBehaviour, ROSTopicSubscriber, ROSSensorConnectionInterface
{
    // Private connection variables
    private ROSBridgeWebSocketConnection ros = null;
    private string client_id;
    private float alpha = 0.8f;
    private Dictionary<string, bool> sensorSubscriberTopicsDict = new Dictionary<string, bool>();

    // List of visualizers
    private Dictionary<string, PCFaceVisualizer> pcFaceVisualizers = new Dictionary<string, PCFaceVisualizer>();

    // Initilize the sensor
    public void InitilizeSensor(int uniqueID, string sensorIP, int sensorPort, List<string> sensorSubscribers)
    {
        Debug.Log("Init PC Face Mesh Connection at IP " + sensorIP + " Port " + sensorPort.ToString());

        ros = new ROSBridgeWebSocketConnection("ws://" + sensorIP, sensorPort);
        client_id = uniqueID.ToString();

        foreach (string subscriber in sensorSubscribers)
        {
            string subscriberTopic = "";

            switch (subscriber)
            {
                default:
                    subscriberTopic = "/" + subscriber;
                    // Create PC Face Visualizer, initilize it as a child of this sensor gameobject and add it to the PCFaceVisualizer dictionary
                    PCFaceVisualizer pcFaceVisualizer = gameObject.AddComponent<PCFaceVisualizer>();
                    pcFaceVisualizer.CreateMeshGameobject(this.transform);
                    pcFaceVisualizers.Add(subscriberTopic, pcFaceVisualizer);
                    break;
            }
            Debug.Log(" PC Face Mesh Subscribing to : " + subscriberTopic);
            sensorSubscriberTopicsDict.Add(subscriberTopic, true);
            ros.AddSubscriber(subscriberTopic, this);
        }

        ros.Connect();

        // Hardcode Parent transform
        //SetLocalPosition(new Vector3(1.98f, 0f, -6.65f));
        //SetLocalOrientation(Quaternion.Euler(0f, 124.654f ,0f));
        //SetLocalScale(new Vector3(0.505388f, 0.505388f, 0.505388f));

    }
    // Update is called once per frame in Unity
    void Update()
    {
        if (ros != null)
        {
            ros.Render();
        }
    }

    public string GetSensorName()
    {
        return this.gameObject.name;
    }

    /// <summary>
    /// Returns a list of connected subscriber topics (which are unique identifiers).
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// Returns a list of connected subscriber topics (which are unique identifiers).
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, bool> GetSensorSubscribers()
    {
        return sensorSubscriberTopicsDict;
    }

    /// <summary>
    /// Function to disconnect a specific subscriber
    /// </summary>
    /// <param name="subscriberID"></param>
    public void Unsubscribe(string subscriberTopic)
    {
        if (sensorSubscriberTopicsDict.ContainsKey(subscriberTopic))
        {
            sensorSubscriberTopicsDict[subscriberTopic] = false;
            ros.RemoveSubscriber(subscriberTopic);
        }
        else
        {
            Debug.Log("No such subscriber exists: " + subscriberTopic);
        }

    }

    /// <summary>
    /// Function to connect a specific subscriber
    /// </summary>
    /// <param name="subscriberID"></param>
    public void Subscribe(string subscriberTopic)
    {
        if (sensorSubscriberTopicsDict.ContainsKey(subscriberTopic))
        {
            if (sensorSubscriberTopicsDict[subscriberTopic] == false)
            {
                ros.AddSubscriber(subscriberTopic, this);
                sensorSubscriberTopicsDict[subscriberTopic] = true;
                return;
            }
        }

        Debug.Log("Subscriber already registered: " + subscriberTopic);
    }

    // ROS Topic Subscriber methods
    public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
    {
        Debug.Log(" PC Face Mesh Recieved message");

        ROSBridgeMsg result = null;
        bool parsePCFaceMesh = false;

        /// Color of the mesh
        Color color = Color.white;


        // Writing all code in here for now. May need to move out to separate handler functions when it gets too unwieldy.
        switch (topic)
        {
            case "/colorized_points_faced_0":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                color = new Color(0, 0, 0.5f, alpha);
                break;
            case "/colorized_points_faced_1":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                color = new Color(0, 0, 1, alpha);
                break;
            case "/colorized_points_faced_2":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                color = new Color(0, 1, 1, alpha);
                break;
            case "/colorized_points_faced_3":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                color = new Color(0, 1, 0, alpha);
                break;
            case "/colorized_points_faced_4":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                color = new Color(1, 1, 0, alpha);
                break;
            case "/colorized_points_faced_5":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                color = new Color(1, 0, 0, alpha);
                break;
            default:
                Debug.LogError("Topic not implemented: " + topic);
                parsePCFaceMesh = true;
                break;
        }

        if (parsePCFaceMesh)
        {
            PCFaceMsg meshMsg = new PCFaceMsg(raw_msg);
            // Obtain visualizer for this topic and update mesh & color
            PCFaceVisualizer visualizer = pcFaceVisualizers[topic];
            visualizer.SetMesh(meshMsg);
            visualizer.SetColor(color);
        }

        return result;
    }
    public string GetMessageType(string topic)
    {
        Debug.Log("PCFace message type is returned as rntools/PCFace by default");
        return "rntools/PCFace";
    }

    public void DisconnectROSConnection()
    {
        ros.Disconnect();
    }

    public void SetLocalOrientation(Quaternion quaternion)
    {
        this.transform.localRotation = quaternion; 
    }

    public void SetLocalPosition(Vector3 position)
    {
        this.transform.localPosition = position;
    }

    public void SetLocalScale(Vector3 scale)
    {
        this.transform.localScale = scale;
    }

}
