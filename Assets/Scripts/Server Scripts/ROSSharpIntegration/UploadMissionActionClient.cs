using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using System;

namespace RosSharp.RosBridgeClient.Actionlib
{

    public class UploadMissionActionClient : ActionClient<MessageTypes.IsaacsServer.UploadMissionAction, MessageTypes.IsaacsServer.UploadMissionActionGoal, MessageTypes.IsaacsServer.UploadMissionActionResult, MessageTypes.IsaacsServer.UploadMissionActionFeedback, MessageTypes.IsaacsServer.UploadMissionGoal, MessageTypes.IsaacsServer.UploadMissionResult, MessageTypes.IsaacsServer.UploadMissionFeedback>
    {
        public string status = "";
        public string feedback = "";
        public string result = "";

        public UploadMissionActionClient(string actionName, RosSocket rosSocket)
        {
            this.actionName = actionName;
            this.rosSocket = rosSocket;
            action = new MessageTypes.IsaacsServer.UploadMissionAction();
            goalStatus = new MessageTypes.Actionlib.GoalStatus();
        }

        protected override MessageTypes.IsaacsServer.UploadMissionActionGoal GetActionGoal()
        {
            return action.action_goal;
        }

        protected override void OnStatusUpdated()
        {
            // Not implemented for this particular application
        }

        protected override void OnFeedbackReceived()
        {
            // Not implemented for this particular application since get string directly returns stored feedback
        }

        protected override void OnResultReceived()
        {
            //TODO: Something but honestly idk what
        }

        public string GetStatusString()
        {
            if (goalStatus != null)
            {
                return ((ActionStatus)(goalStatus.status)).ToString();
            }
            return "";
        }

        public string GetFeedbackString()
        {
            if (action != null)
                return String.Join(",", action.action_feedback.feedback);
            return "";
        }

        public string GetResultString()
        {
            if (action != null)
                return String.Join(",", action.action_result.result);
            return "";
        }
    }
}
