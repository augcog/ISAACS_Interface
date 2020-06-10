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

public class PCFaceSensor_ROSSensorConnection : MonoBehaviour, ROSTopicSubscriber {
    // Visualizer variables
    public static string rendererObjectName = "PlacementPlane"; // pick a center point of the map, ideally as part of rotating map

    // Private connection variables
    private ROSBridgeWebSocketConnection ros = null;
    public string client_id;

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
                case "mesh":
                    subscriberTopic = "/voxblox_node/" + subscriber;
                    break;
                default:
                    subscriberTopic = "/" + subscriber;
                    break;
            }
            Debug.Log(" PC Face Mesh Subscribing to : " + subscriberTopic);
            ros.AddSubscriber(subscriberTopic, this);
        }

        gameObject.AddComponent<PCFaceVisualizer>();        
        Debug.Log("PC Face Mesh Connection Established");
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

    // ROS Topic Subscriber methods
    public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
    {
        Debug.Log(" PC Face Mesh Recieved message");

        ROSBridgeMsg result = null;
        bool parsePCFaceMesh = false;

        // Writing all code in here for now. May need to move out to separate handler functions when it gets too unwieldy.
        switch (topic)
        {
            case "/colorized_points_faced_0":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                break;
            case "/colorized_points_faced_1":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                break;
            case "/colorized_points_faced_2":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                break;
            case "/colorized_points_faced_3":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                break;
            case "/colorized_points_faced_4":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                break;
            case "/colorized_points_faced_5":
                Debug.Log("PC Face Mesh Visualizer Callback: " + topic);
                parsePCFaceMesh = true;
                break;
            default:
                Debug.LogError("Topic not implemented: " + topic);
                parsePCFaceMesh = true;
                break;
        }

        if (parsePCFaceMesh)
        {
            PCFaceMsg meshMsg = new PCFaceMsg(raw_msg);
            PCFaceVisualizer visualizer = this.gameObject.GetComponent<PCFaceVisualizer>();
            visualizer.SetMesh(meshMsg);
        }

        return result;
    }
    public string GetMessageType(string topic)
    {
        Debug.Log("PCFace message type is returned as rntools/PCFace by default");
        return "rntools/PCFace";
    }

}
