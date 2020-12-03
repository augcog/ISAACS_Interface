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

// TODO: Integrate Colour Logic

public class LampSensor_v2 : MonoBehaviour, ROSSensorConnectionInterface, LampSensor_ROSSensorConnection
{
    // Helper enum
    public enum PointCloudLevel
    {
        RED = 0,
        ORANGE = 1,
        YELLOW = 2,
        GREEN = 3,
        BLUE = 4,
        LIGHT_BLUE = 5,
        WHITE = 6
    }

    public PointCloudLevel pointCloudLevel = PointCloudLevel.WHITE;
    private Dictionary<string, PointCloudVisualizer> pcVisualizers = new Dictionary<string, PointCloudVisualizer>();

    // Private connection variables
    //private ROSBridgeWebSocketConnection ros = null;
    private Boolean setup = false;
    private string client_id;
    private Dictionary<string, bool> sensorSubscriberTopicsDict = new Dictionary<string, bool>();

    // Initilize the sensor
    public void InitilizeSensor(int uniqueID, string sensorURL, int sensorPort, List<string> sensorSubscribers)
    {
        Debug.Log("Init LAMP Connection at " + sensorURL + ":" + sensorPort.ToString());
        setup = true;
        client_id = uniqueID.ToString();

        foreach (string subscriber in sensorSubscribers)
        {
            string subscriberTopic = "";

            switch (subscriber)
            {
                case "surface_pointcloud":
                    subscriberTopic = "/voxblox_node/" + subscriber;
                    break;
                default:
                    subscriberTopic = "/" + subscriber;
                    break;
            }

            PointCloudVisualizer pcVisualizer = gameObject.AddComponent<PointCloudVisualizer>();
            pcVisualizer.SetParentTransform(this.transform);
            pcVisualizers.Add(subscriberTopic, pcVisualizer);
            Debug.Log(" LAMP Subscribing to : " + subscriberTopic);
            sensorSubscriberTopicsDict.Add(subscriberTopic, true);
            //ros.AddSubscriber(subscriberTopic, this);
            Subscribe(subscriberTopic);
        }

        Debug.Log("Lamp Connection Established");
        ros.Connect();
        //ServerConnections.rosServerConnection.Connect();
    }
    // Update is called once per frame in Unity
    void Update()
    {
        if (setup)
        {
            ros.Render();
            //ServerConnections.rosServerConnection.Render();
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
            //TODO: how to let serve know we need to subscribe/unsubscribe
            //ros.RemoveSubscriber(subscriberTopic);
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
                //TODO: how to let serve know we need to subscribe/unsubscribe
                //ros.AddSubscriber(subscriberTopic, this);
                sensorSubscriberTopicsDict[subscriberTopic] = true;
                return;
            }
        }

        Debug.Log("Subscriber already registered: " + subscriberTopic);
    }

    // ROS Topic Subscriber methods
    public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
    {
        Debug.Log(" LAMP Recieved message");

        ROSBridgeMsg result = null;
        // Writing all code in here for now. May need to move out to separate handler functions when it gets too unwieldy.
        switch (topic)
        {
            case "/voxblox_node/surface_pointcloud":
                Debug.Log(" LAMP Recieved surface point cloud message");
                pointCloudLevel = PointCloudLevel.WHITE;
                break;
            case "/colorized_points_0":
                pointCloudLevel = PointCloudLevel.RED;
                break;
            case "/colorized_points_1":
                pointCloudLevel = PointCloudLevel.ORANGE;
                break;
            case "/colorized_points_2":
                pointCloudLevel = PointCloudLevel.YELLOW;
                break;
            case "/colorized_points_3":
                pointCloudLevel = PointCloudLevel.GREEN;
                break;
            case "/colorized_points_4":
                pointCloudLevel = PointCloudLevel.BLUE;
                break;
            case "/colorized_points_5":
                pointCloudLevel = PointCloudLevel.LIGHT_BLUE;
                break;
            default:
                Debug.LogError("Topic not implemented: " + topic);
                return result;
        }

        PointCloud2Msg pointCloudMsg = new PointCloud2Msg(raw_msg);
        this.pcVisualizers[topic].PointCloudLevel = pointCloudLevel;
        this.pcVisualizers[topic].SetPointCloud(pointCloudMsg.GetCloud());
        Debug.Log("Updated Point Cloud");
        return result;
    }
    public string GetMessageType(string topic)
    {
        Debug.Log("Point Cloud message type is returned as sensor_msg/PointCloud2 by default");
        return "sensor_msgs/PointCloud2";
        /**
        switch (topic)
        {
            case "/dji_sdk/attitude":
                return "geometry_msgs/QuaternionStamped";
            case "/dji_sdk/battery_state":
                return "sensor_msgs/BatteryState";
            case "/dji_sdk/flight_status":
                return "std_msgs/UInt8";
            case "/dji_sdk/gimbal_angle":
                return "geometry_msgs/Vector3Stamped";
            case "/dji_sdk/gps_health":
                return "std_msgs/UInt8";
            case "/dji_sdk/gps_position":
                return "sensor_msgs/NavSatFix";
            case "/dji_sdk/imu":
                return "sensor_msgs/Imu";
            case "/dji_sdk/rc":
                return "sensor_msgs/Joy";
            case "/dji_sdk/velocity":
                return "geometry_msgs/Vector3Stamped";
            case "/dji_sdk/height_above_takeoff":
                return "std_msgs/Float32";
            case "/dji_sdk/local_position":
                return "geometry_msgs/PointStamped";
        }
        Debug.LogError("Topic " + topic + " not registered.");
        return "";

        **/
    }

    public void DisconnectROSConnection()
    {
        ros.Disconnect();
        //ServerConnections.rosServerConnection.Disconnect();
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
