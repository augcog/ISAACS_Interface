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
    // Private connection variables
    private ROSBridgeWebSocketConnection ros = null;
    string client_id;

    private Dictionary<string, PointCloudVisualizer> pcVisualizers = new Dictionary<string, PointCloudVisualizer>();

    // Initilize the sensor
    public void InitilizeSensor(int uniqueID, string sensorIP, int sensorPort, List<string> sensorSubscribers)
    {
        ros = new ROSBridgeWebSocketConnection("ws://" + sensorIP, sensorPort);
        client_id = uniqueID.ToString();

        foreach (string subscriber in sensorSubscribers)
        {
            string subscriberTopic = "";

            subscriberTopic = "/voxblox_node/" + subscriber;
            PointCloudVisualizer pcVisualizer = gameObject.AddComponent<PointCloudVisualizer>();
            pcVisualizer.SetParentTransform(this.transform);
            pcVisualizers.Add(subscriberTopic, pcVisualizer);
            Debug.Log("Point Cloud subscribing to: " + subscriberTopic);
            ros.AddSubscriber("/voxblox_node/" + subscriber, this);
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
                this.pcVisualizers[topic].SetPointCloud(pointCloudMsg.GetCloud());
                Debug.Log("Updated Point Cloud");
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
        return "sensor_msgs/PointCloud2";
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
