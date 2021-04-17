using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.Actionlib
{
    [RequireComponent(typeof(RosConnector))]
    public class UnitySetSpeedActionClient : MonoBehaviour
    {
        private RosConnector rosConnector;
        public SetSpeedActionClient setSpeedActionClient;

        public string actionName;
        public float speed;
        public int id;
        public string status = "";
        public string feedback = "";
        public string result = "";

        private void Start()
        {
            rosConnector = GetComponent<RosConnector>();
            setSpeedActionClient = new SetSpeedActionClient(actionName, rosConnector.RosSocket);
            setSpeedActionClient.Initialize();
        }

        private void Update()
        {
            status = setSpeedActionClient.GetStatusString();
            feedback = setSpeedActionClient.GetFeedbackString();
            result = setSpeedActionClient.GetResultString();
        }

        public void RegisterGoal()
        {
            setSpeedActionClient.speed = speed;
            setSpeedActionClient.id = id;
        }

    }
}

