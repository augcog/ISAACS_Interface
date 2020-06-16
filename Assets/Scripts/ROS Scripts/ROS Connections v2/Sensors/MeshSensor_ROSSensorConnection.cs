using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System.Threading;


using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.sensor_msgs;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using ROSBridgeLib.voxblox_msgs;

using ISAACS;

public class MeshSensor_ROSSensorConnection : MonoBehaviour, ROSTopicSubscriber {
    
    // Visualizer variables
    public static string rendererObjectName = "PlacementPlane"; // pick a center point of the map, ideally as part of rotating map

    // Private connection variables
    private ROSBridgeWebSocketConnection ros = null;
    public string client_id;
    private Thread rosMsgThread;

    // Queue of jsonMsgs to be parsed on thread
    private Queue<JSONNode> jsonMsgs = new Queue<JSONNode>();

    // Queue of meshMsgs parsed by thread and to be visualized
    private Queue<MeshMsg> meshMsgs = new Queue<MeshMsg>();

    private MeshVisualizer visualizer;

    private void CreateThread()
    {
        rosMsgThread = new Thread( new ThreadStart(ParseJSON));
        rosMsgThread.IsBackground = true;
        rosMsgThread.Start();
    }

    private void ParseJSON()
    {
        while (true)
        {
            // Check if any json msgs have been recieved
            if (jsonMsgs.Count != 0)
            {
                // Parse json msg to mesh msg
                JSONNode rawMsg = jsonMsgs.Dequeue();
                MeshMsg meshMsg = new MeshMsg(rawMsg);
                meshMsgs.Enqueue(meshMsg);
            }
        }
    }

    // Initilize the sensor
    public void InitilizeSensor(int uniqueID, string sensorIP, int sensorPort, List<string> sensorSubscribers)
    {
        Debug.Log("Init Mesh Connection at IP " + sensorIP + " Port " + sensorPort.ToString());

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
            Debug.Log(" Mesh Subscribing to : " + subscriberTopic);
            ros.AddSubscriber(subscriberTopic, this);
        }

        Debug.Log("Mesh Connection Established");
        ros.Connect();

        // Initilize visualizer
        visualizer = GameObject.Find(rendererObjectName).GetComponent<MeshVisualizer>();
    }
    // Update is called once per frame in Unity
    void Update()
    {
        if (ros != null)
        {
            ros.Render();
        }

        // Check if any mesh msgs are available to be visualized
        if (meshMsgs.Count > 0)
        {
            MeshMsg meshMsg = meshMsgs.Dequeue();
            visualizer.SetMesh(meshMsg);
        }
    }

    // ROS Topic Subscriber methods
    public ROSBridgeMsg OnReceiveMessage(string topic, JSONNode raw_msg, ROSBridgeMsg parsed = null)
    {
        Debug.Log(" Mesh Recieved message");

        ROSBridgeMsg result = null;
        // Writing all code in here for now. May need to move out to separate handler functions when it gets too unwieldy.
        switch (topic)
        {
            case "/voxblox_node/surface_pointcloud":
                Debug.Log("Mesh Visualizer Callback.");
                // Add raw_msg to the jsonMsgs to be parsed on the thread
                jsonMsgs.Enqueue(raw_msg);

                //MeshMsg meshMsg =  new MeshMsg(raw_msg);
                //MeshVisualizer visualizer = GameObject.Find(rendererObjectName).GetComponent<MeshVisualizer>();
                //visualizer.SetMesh(meshMsg);
                break;
            default:
                Debug.LogError("Topic not implemented: " + topic);
                break;
        }
        return result;
    }
    public string GetMessageType(string topic)
    {
        Debug.Log("Mesh message type is returned as voxblox_msgs/Mesh by default");
        return "voxblox_msgs/Mesh";
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


}
