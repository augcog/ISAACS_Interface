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
        public string status = "";
        public string feedback = "";
        public string result = "";

        private void Start()
        {
            rosConnector = GetComponent<RosConnector>();
            uploadMissionActionClient = new UploadMissionActionClient(actionName, rosConnector.RosSocket);
            uploadMissionActionClient.Initialize();
        }

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

        public void RegisterGoal()
        {
            Debug.Log("registered Goal");
        }

        public void sendGoal(UploadMissionGoal goal)
        {
            
            uploadMissionActionClient.SendGoal();
        }
    }
}
