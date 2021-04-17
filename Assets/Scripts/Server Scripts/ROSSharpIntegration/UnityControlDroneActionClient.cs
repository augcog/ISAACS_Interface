using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.Actionlib
{
    [RequireComponent(typeof(RosConnector))]
    public class UnityControlDroneActionClient : MonoBehaviour
    {
        private RosConnector rosConnector;
        public ControlDroneActionClient controlDroneActionClient;

        public string actionName;
        public string command;
        public int id;
        public string status = "";
        public string feedback = "";
        public string result = "";

        private void Start()
        {
            rosConnector = GetComponent<RosConnector>();
            controlDroneActionClient = new ControlDroneActionClient(actionName, rosConnector.RosSocket);
            controlDroneActionClient.Initialize();
        }

        private void Update()
        {
            status = controlDroneActionClient.GetStatusString();
            feedback = controlDroneActionClient.GetFeedbackString();
            result = controlDroneActionClient.GetResultString();
        }

        public void RegisterGoal()
        {
            controlDroneActionClient.command = command;
            controlDroneActionClient.id = id;
        }

    }
}

