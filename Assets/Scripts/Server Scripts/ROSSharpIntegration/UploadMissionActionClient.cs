using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using System;

namespace RosSharp.RosBridgeClient.Actionlib
{

    public class UploadMissionActionClient : ActionClient<MessageTypes.IsaacsServer.UploadMissionAction,
                                                          MessageTypes.IsaacsServer.UploadMissionActionGoal,
                                                          MessageTypes.IsaacsServer.UploadMissionActionResult,
                                                          MessageTypes.IsaacsServer.UploadMissionActionFeedback,
                                                          MessageTypes.IsaacsServer.UploadMissionGoal,
                                                          MessageTypes.IsaacsServer.UploadMissionResult,
                                                          MessageTypes.IsaacsServer.UploadMissionFeedback>
    {
        public NavSatFix[] waypoints;    // Drone waypoints
        public string status = "";
        public string feedback = "";
        public string result = "";
        public int id;

        /// <summary>
        /// Initializes the UploadMission action, status & rosSocket
        /// </summary>
        public UploadMissionActionClient(string actionName, RosSocket rosSocket)
        {
            this.actionName = actionName;
            this.rosSocket = rosSocket;
            action = new MessageTypes.IsaacsServer.UploadMissionAction();
            goalStatus = new MessageTypes.Actionlib.GoalStatus();
        }

        /// <summary>
        /// Sets and gets uploadMission's actionGoal
        /// </summary>
        protected override MessageTypes.IsaacsServer.UploadMissionActionGoal GetActionGoal()
        {
            action.action_goal.goal.id = (uint)id;
            action.action_goal.goal.waypoints = waypoints;
            return action.action_goal;
        }

        /// <summary>
        /// Sends UploadMissionActionGoal using calls to ActionClient
        /// </summary>
        public void SendGoal(MessageTypes.IsaacsServer.UploadMissionActionGoal goal)
        {
            action.action_goal = goal;
            action.action_goal.header = goal.header;
            action.action_goal.goal = goal.goal;
            action.action_goal.goal.waypoints = goal.goal.waypoints;
            action.action_goal.goal.id = goal.goal.id;
            SendGoal();
        }

        protected override void OnStatusUpdated()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnFeedbackReceived()
        {
            // Not implemented for this particular application since get string directly returns stored feedback
            Debug.Log("Feedback Received");
        }

        /// <summary>
        /// Attaches waypoints upon receiving result from the action server
        /// </summary>
        protected override void OnResultReceived()
        {
            Debug.Log("UploadMission Result Received");
            int drone_id = (int)action.action_result.result.id;
            string command = action.action_result.result.message;
            Debug.Log(command);
            bool success = action.action_result.result.success;
            Drone_v2 drone = WorldProperties.GetDroneDict()[drone_id];

            if (success)
            {
                int continueFromWaypointID = drone.droneProperties.CurrentWaypointTargetID() + 1;
                List<Waypoint> ways = drone.AllWaypoints();
                foreach (Waypoint way in ways)
                {
                    Vector3 waypoint_coord = way.gameObjectPointer.transform.localPosition;
                    float distance = Vector3.Distance(way.gameObjectPointer.transform.localPosition, waypoint_coord);

                    if (distance < 0.2f)
                    {
                        way.waypointProperties.WaypointUploaded();
                    }
                }
                drone.droneProperties.StartCheckingFlightProgress(continueFromWaypointID, ways.Count);

            }
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
                return String.Join(",", action.action_feedback.feedback);
            return "";
        }

        /// <summary>
        /// Gets result
        /// </summary>
        public string GetResultString()
        {
            if (action != null)
                return String.Join(",", action.action_result.result);
            return "";
        }
    }
}
