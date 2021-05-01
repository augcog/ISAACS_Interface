using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using RosSharp.RosBridgeClient.MessageTypes.Std;
using RosSharp.RosBridgeClient.MessageTypes.Actionlib;
using RosSharp.RosBridgeClient.MessageTypes.IsaacsServer;
namespace RosSharp.RosBridgeClient.Actionlib
{

    public class UnityUploadMissionActionClient : MonoBehaviour
    {
        private RosConnector rosConnector;
        public UploadMissionActionClient uploadMissionActionClient;

        public string actionName;
        public int id;
        public MessageTypes.Sensor.NavSatFix[] waypoints;    // Drone waypoints
        public string status = "";
        public string feedback = "";
        public string result = "";

        /// <summary>
        /// Instantiating UploadMissionActionClient using Unity editor
        /// </summary>
        private void Start()
        {
            rosConnector = GetComponent<RosConnector>();
            uploadMissionActionClient = new UploadMissionActionClient(actionName, rosConnector.RosSocket);
            uploadMissionActionClient.Initialize();
        }

        /// <summary>
        /// Update fields on the inspector through UploadMissionActionClient getter calls
        /// </summary>
        private void Update()
        {
            status = uploadMissionActionClient.GetStatusString();
            feedback = uploadMissionActionClient.GetFeedbackString();
            result = uploadMissionActionClient.GetResultString();
        }

        public void RegisterGoal(UploadMissionGoal goal)
        {
            uploadMissionActionClient.action.action_goal.goal = goal;

        }

        /// <summary>
        /// Binds drone waypoints & id from the inspector to UploadMissionActionClient object
        /// </summary>
        public void RegisterGoal()
        {
            Debug.Log("registered Goal");
            uploadMissionActionClient.id = id;
            uploadMissionActionClient.waypoints = waypoints;
        }

        /// <summary>
        /// Sends goal through call from UploadMissionActionClient
        /// </summary>
        public void sendGoal(UploadMissionGoal goal)
        {
            
            uploadMissionActionClient.SendGoal();
        }
    }
}
