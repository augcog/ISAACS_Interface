﻿using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

using ROSBridgeLib;
using ROSBridgeLib.sensor_msgs;

public class CameraSensor_ROSSensorConnection : MonoBehaviour, ROSTopicSubscriber, ROSSensorConnectionInterface
{
    // Private connection variables
    private ROSBridgeWebSocketConnection ros = null;
    private string client_id;
    private float alpha = 0.8f;
    private List<string> sensorSubscriberTopics = new List<string>();
    private string videoType;
    private Dictionary<string,bool> keyList = new Dictionary<string, bool>();

    // List of visualizers
    private Dictionary<string, ImageVisualizer> imageVisualizers = new Dictionary<string, ImageVisualizer>();

    // Initialize the sensor
    public void InitilizeSensor(int uniqueID, string sensorURL, int sensorPort, List<string> sensorSubscribers)
    {
        Debug.Log("Init Camera Connection at " + sensorURL + ":" + sensorPort.ToString());

        ros = new ROSBridgeWebSocketConnection(sensorURL, sensorPort);
        client_id = uniqueID.ToString();
        foreach (string subscriber in sensorSubscribers)
        {
            string subscriberTopic = "";
            switch (subscriber)
            {
                default:
                    subscriberTopic = "/dji_sdk/" + subscriber;
                    ImageVisualizer imageVisualizer = new ImageVisualizer();
                    //imageVisualizer.SetParentTransform(this.transform);
                    imageVisualizers.Add(subscriberTopic, imageVisualizer);
                    Debug.Log("Camera subscribing to: " + subscriberTopic);
                    ros.AddSubscriber("/dji_sdk/" + subscriber, this);
                    break;
            }
            Debug.Log("Camera Subscribing to : " + subscriberTopic);
            sensorSubscriberTopics.Add(subscriberTopic);
            keyList.Add(subscriberTopic, true);
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

    public string GetSensorName()
    {
        return this.gameObject.name;
    }

    public string GetVideoType()
    {
        return videoType;
    }

    public void SetVideoType(string input)
    {
        videoType = input;
    }
    /// <summary>
    /// Returns a list of connected subscriber topics (which are unique identifiers).
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, bool> GetSensorSubscribers()
    {
        return keyList;
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
        this.imageVisualizers[topic].SetFrame(meshMsg, videoType);
        return result;
    }
    public string GetMessageType(string topic)
    {
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
