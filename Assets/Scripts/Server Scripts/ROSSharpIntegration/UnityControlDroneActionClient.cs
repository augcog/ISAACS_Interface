﻿using System.Collections;
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
        public string command;    // Drone mission call
        public int id;
        public string status = "";
        public string feedback = "";
        public string result = "";

        // Instantiating ControlDroneActionClient using Unity editor
        private void Start()
        {
            rosConnector = GetComponent<RosConnector>();
            controlDroneActionClient = new ControlDroneActionClient(actionName, rosConnector.RosSocket);
            controlDroneActionClient.Initialize();
        }

        // Update fields on the inspector
        private void Update()
        {
            status = controlDroneActionClient.GetStatusString();
            feedback = controlDroneActionClient.GetFeedbackString();
            result = controlDroneActionClient.GetResultString();
        }

        // Binds drone command & id from the inspector to ControlDroneActionClient object
        public void RegisterGoal()
        {
            controlDroneActionClient.command = command;
            controlDroneActionClient.id = id;
        }

    }
}

