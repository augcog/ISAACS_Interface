using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using RosSharp.RosBridgeClient.MessageTypes.IsaacsServer;
using System;

namespace RosSharp.RosBridgeClient.Actionlib
{

    public class ControlDroneActionClient : ActionClient<MessageTypes.IsaacsServer.ControlDroneAction,
                                                         MessageTypes.IsaacsServer.ControlDroneActionGoal,
                                                         MessageTypes.IsaacsServer.ControlDroneActionResult,
                                                         MessageTypes.IsaacsServer.ControlDroneActionFeedback,
                                                         MessageTypes.IsaacsServer.ControlDroneGoal,
                                                         MessageTypes.IsaacsServer.ControlDroneResult,
                                                         MessageTypes.IsaacsServer.ControlDroneFeedback>
    {
        public string command;
        public string status = "";
        public string feedback = "";
        public string result = "";

        public ControlDroneActionClient(string actionName, RosSocket rosSocket)
        {
            this.actionName = actionName;
            this.rosSocket = rosSocket;
            action = new ControlDroneAction();
            goalStatus = new MessageTypes.Actionlib.GoalStatus();
        }

        protected override ControlDroneActionGoal GetActionGoal()
        {
            action.action_goal.goal.control_task = command;
            return action.action_goal;
        }

        protected override void OnFeedbackReceived()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnResultReceived()
        {
            Debug.Log("ControlDrone Result Received");

            int drone_id = (int) action.action_result.result.id;
            string command = action.action_result.result.message;
            Drone_v2 drone = WorldProperties.GetDroneDict()[drone_id];

            switch (command)
            {
                case "start_mission":
                    drone.onStartMission();
                    break;

                case "pause_mission":
                    drone.onPauseMission();
                    break;

                case "resume_mission":
                    drone.onResumeMission();
                    break;

                case "land_drone":
                    drone.onLandDrone();
                    break;

                case "fly_home":
                    drone.onFlyHome();
                    break;

                default:
                    Debug.Log("Wrong drone callback!");
                    break;
            }
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
