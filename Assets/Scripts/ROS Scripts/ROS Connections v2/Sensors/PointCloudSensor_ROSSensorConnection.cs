using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;

using ISAACS;

public class PointCloudSensor_ROSSensorConnection : MonoBehaviour, ROSTopicSubscriber, ROSSensorConnectionInterface
{
    // Visualizer variables
    public static string rendererObjectName = "PlacementPlane"; // pick a center point of the map, ideally as part of rotating map

    // Private connection variables
    private ROSBridgeWebSocketConnection ros = null;
    private string client_id;
    private List<string> sensorSubscriberTopics = new List<string>();

    // Initilize the sensor
    public void InitilizeSensor(int uniqueID, string sensorIP, int sensorPort, List<string> sensorSubscribers)
    {
        ros = new ROSBridgeWebSocketConnection("ws://" + sensorIP, sensorPort);
        client_id = uniqueID.ToString();

        foreach (string subscriber in sensorSubscribers)
        {
            Debug.Log(" Point cloud Subscribing to : " + subscriber);
            string subscriberTopic = "/voxblox_node/" + subscriber;
            ros.AddSubscriber(subscriberTopic, this);
            sensorSubscriberTopics.Add(subscriberTopic);
        }
        ros.Connect();

    }

    // Update is called once per frame in Unity
    void Update()
    {
        if (ros != null)
        {
            ros.Render();
        }
    }


    /// <summary>
    /// Returns a list of connected subscriber topics (which are unique identifiers).
    /// </summary>
    /// <returns></returns>
    public List<string> GetSensorSubscribers()
    {
        return sensorSubscriberTopics;
    }

    /// <summary>
    /// Function to disconnect a specific subscriber
    /// </summary>
    /// <param name="subscriberID"></param>
    public void Unsubscribe(string subscriberTopic)
    {
        if (sensorSubscriberTopics.Contains(subscriberTopic))
        {
            sensorSubscriberTopics.Remove(subscriberTopic);
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
        if (sensorSubscriberTopics.Contains(subscriberTopic) == false)
        {
            ros.AddSubscriber(subscriberTopic, this);
        }
        else
        {
            Debug.Log("Subscriber already registered: " + subscriberTopic);
        }
    }

    //Get State variables
    public string GetID()
    {
        return client_id;
    }

    // ROS Topic Subscriber methods
    public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
    {
        ROSBridgeMsg result = null;
        // Writing all code in here for now. May need to move out to separate handler functions when it gets too unwieldy.
        switch (topic)
        {
            case "/voxblox_node/surface_pointcloud":
                PointCloud2Msg pointCloudMsg = new PointCloud2Msg(raw_msg);
                PointCloudVisualizer(pointCloudMsg);
                break;
            default:
                Debug.LogError("Topic not implemented: " + topic);
                break;
        }
        return result;
    }
    public string GetMessageType(string topic)
    {
        Debug.Log("Point Cloud message type is returned as sensor_msg/PointCloud2 by deafault");
        return "sensor_msg/PointCloud2";
    }

    // Visualizer helper scripts
    private void PointCloudVisualizer( PointCloud2Msg pointCloudMsg)
    {
        // Idea: We could have each sesnor have a PointCloudVisualizer attached to it and use that one.
        PointCloudVisualizer visualizer = GameObject.Find(rendererObjectName).GetComponent<PointCloudVisualizer>();
        visualizer.SetPointCloud(pointCloudMsg.GetCloud());
        Debug.Log("Updated Point Cloud");
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
