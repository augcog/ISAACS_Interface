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
        public string command;    // Drone mission call
        public int id;
        public string status = "";
        public string feedback = "";
        public string result = "";
 
        /// <summary>
        /// Initializes the ControlDrone action, status & rosSocket
        /// </summary>
        public ControlDroneActionClient(string actionName, RosSocket rosSocket)
        {
            this.actionName = actionName;
            this.rosSocket = rosSocket;
            action = new ControlDroneAction();
            goalStatus = new MessageTypes.Actionlib.GoalStatus();
        }

        /// <summary>
        /// Sets and gets controlDrone's actionGoal
        /// </summary>
        protected override ControlDroneActionGoal GetActionGoal()
        {
            action.action_goal.goal.control_task = command;
            action.action_goal.goal.id = (uint) id;
            return action.action_goal;
        }

        protected override void OnFeedbackReceived()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Runs drone mission upon receiving result from the action server
        /// </summary>
        protected override void OnResultReceived()
        {
            Debug.Log("ControlDrone Result Received");
            Debug.Log(action.action_result.result.message);
            int drone_id = (int) action.action_result.result.id;
            bool success = action.action_result.result.success;

            //TODO: Check this!!
            string command = action.action_result.result.control_task;
            Drone_v2 drone = WorldProperties.GetDroneDict()[drone_id];

            if (success)
            {
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
            } else
            {
                Debug.Log("Not successful command.");
            }
      
        }

        protected override void OnStatusUpdated()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets status
        /// </summary>
        public string GetStatusString()
        {
            if (goalStatus != null)
            {
                return ((ActionStatus)(goalStatus.status)).ToString();
            }
            return "";
        }

        /// <summary>
        /// Gets feedback (not implemented for this application)
        /// </summary>
        public string GetFeedbackString()
        {
            if (action != null)
                return String.Join(",", action.action_feedback.feedback.progress);
            return "";
        }

        /// <summary>
        /// Gets result
        /// </summary>
        public string GetResultString()
        {
            if (action != null)
                return String.Join(",", action.action_result.result.message);
            return "";
        }
    }
}
