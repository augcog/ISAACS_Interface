using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using System;

namespace RosSharp.RosBridgeClient.Actionlib
{

    public class UploadMissionActionClient : ActionClient<MessageTypes.IsaacsServer.UploadMissionAction, MessageTypes.IsaacsServer.UploadMissionActionGoal, MessageTypes.IsaacsServer.UploadMissionActionResult, MessageTypes.IsaacsServer.UploadMissionActionFeedback, MessageTypes.IsaacsServer.UploadMissionGoal, MessageTypes.IsaacsServer.UploadMissionResult, MessageTypes.IsaacsServer.UploadMissionFeedback>
    {
        public NavSatFix[] waypoints;
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

        protected override void OnResultReceived()
        {
            //TODO: Something but honestly idk what
            Debug.Log("UploadMission Result Received");

            int drone_id = (int)action.action_result.result.id;
            string command = action.action_result.result.message;
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
