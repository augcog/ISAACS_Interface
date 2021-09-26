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

        public string actionName;    // Drone speed
        public float speed;
        public int id;
        public string status = "";
        public string feedback = "";
        public string result = "";

        /// <summary>
        /// Instantiating SetSpeedActionClient using Unity editor
        /// </summary>
        private void Start()
        {
            rosConnector = GetComponent<RosConnector>();
            setSpeedActionClient = new SetSpeedActionClient(actionName, rosConnector.RosSocket);
            setSpeedActionClient.Initialize();
        }

        /// <summary>
        /// Update fields on the inspector through SetSpeedActionClient getter calls
        /// </summary>
        private void Update()
        {
            status = setSpeedActionClient.GetStatusString();
            feedback = setSpeedActionClient.GetFeedbackString();
            result = setSpeedActionClient.GetResultString();
        }
        
        /// <summary>
        /// Binds drone speed & id from the inspector to SetSpeedActionClient object
        /// </summary>
        public void RegisterGoal()
        {
            setSpeedActionClient.speed = speed;
            setSpeedActionClient.id = id;
        }

    }
}

