using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

using ROSBridgeLib;
using ROSBridgeLib.sensor_msgs;

public class CameraSensor_ROSSensorConnection : MonoBehaviour, ROSTopicSubscriber, ROSSensorConnectionInterface
{
    //Note: Copied from PCFACEsensor 
    // Private connection variables
    private ROSBridgeWebSocketConnection ros = null;
    private string client_id;
    private float alpha = 0.8f;
    private List<string> sensorSubscriberTopics = new List<string>();

    // List of visualizers
    private Dictionary<string, ImageVisualizer> imageVisualizers = new Dictionary<string, ImageVisualizer>();

    // Initilize the sensor
    public void InitilizeSensor(int uniqueID, string sensorIP, int sensorPort, List<string> sensorSubscribers)
    {
        Debug.Log("Init Camera Connection at IP " + sensorIP + " Port " + sensorPort.ToString());

        ros = new ROSBridgeWebSocketConnection("ws://" + sensorIP, sensorPort);
        client_id = uniqueID.ToString();

        foreach (string subscriber in sensorSubscribers)
        {
            string subscriberTopic = "";

            switch (subscriber)
            {
                default:
                    subscriberTopic = "/camera/" + subscriber;
                    ImageVisualizer imageVisualizer = gameObject.AddComponent<ImageVisualizer>();
                    //imageVisualizer.SetParentTransform(this.transform);
                    imageVisualizers.Add(subscriberTopic, imageVisualizer);
                    Debug.Log("Camera subscribing to: " + subscriberTopic);
                    ros.AddSubscriber("/camera/" + subscriber, this);
                    break;
            }
            Debug.Log("Camera Subscribing to : " + subscriberTopic);
            sensorSubscriberTopics.Add(subscriberTopic);
        }

        ros.Connect();

        //// Hardcode Parent transform
        //SetLocalPosition(new Vector3(1.98f, 0f, -6.65f));
        //SetLocalOrientation(Quaternion.Euler(0f, 124.654f, 0f));
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

    // ROS Topic Subscriber methods
    public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
    {
        Debug.Log("Camera Recieved message");

        ROSBridgeMsg result = null;
      
        ImageMsg meshMsg = new ImageMsg(raw_msg);
        // Obtain visualizer for this topic
        ImageVisualizer visualizer = imageVisualizers[topic];
        this.imageVisualizers[topic].SetFrame(meshMsg);

        //TODO: change the gameobject's texture2D with visualizer.GetData();

        return result;
    }
    public string GetMessageType(string topic)
    {
        //Debug.Log("PCFace message type is returned as rntools/PCFace by default");
        return "sensor_msgs/Image";
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
