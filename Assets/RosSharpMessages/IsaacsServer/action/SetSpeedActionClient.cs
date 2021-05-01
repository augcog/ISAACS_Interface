﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using RosSharp.RosBridgeClient.MessageTypes.IsaacsServer;
using System;

namespace RosSharp.RosBridgeClient.Actionlib
{

    public class SetSpeedActionClient : ActionClient<MessageTypes.IsaacsServer.SetSpeedAction,
                                                     MessageTypes.IsaacsServer.SetSpeedActionGoal,
                                                     MessageTypes.IsaacsServer.SetSpeedActionResult,
                                                     MessageTypes.IsaacsServer.SetSpeedActionFeedback,
                                                     MessageTypes.IsaacsServer.SetSpeedGoal,
                                                     MessageTypes.IsaacsServer.SetSpeedResult,
                                                     MessageTypes.IsaacsServer.SetSpeedFeedback>
    {
        public float speed;    // Drone speed
        public int id;
        public string status = "";
        public string feedback = "";
        public string result = "";

        // Initializes the SetSpeed action, status & rosSocket
        public SetSpeedActionClient(string actionName, RosSocket rosSocket)
        {
            this.actionName = actionName;
            this.rosSocket = rosSocket;
            action = new SetSpeedAction();
            goalStatus = new MessageTypes.Actionlib.GoalStatus();
        }

        protected override SetSpeedActionGoal GetActionGoal()
        {
            action.action_goal.goal.speed = speed;
            action.action_goal.goal.id = (uint) id;
            return action.action_goal;
        }

        protected override void OnFeedbackReceived()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnResultReceived()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnStatusUpdated()
        {
            throw new System.NotImplementedException();
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
                return String.Join(",", action.action_feedback.feedback.progress);
            return "";
        }

        public string GetResultString()
        {
            if (action != null)
                return String.Join(",", action.action_result.result.message);
            return "";
        }
    }
}
