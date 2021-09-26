/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



namespace RosSharp.RosBridgeClient.MessageTypes.IsaacsServer
{
    public class ControlDroneAction : Action<ControlDroneActionGoal, ControlDroneActionResult, ControlDroneActionFeedback, ControlDroneGoal, ControlDroneResult, ControlDroneFeedback>
    {
        public const string RosMessageName = "isaacs_server/ControlDroneAction";

        public ControlDroneAction() : base()
        {
            this.action_goal = new ControlDroneActionGoal();
            this.action_result = new ControlDroneActionResult();
            this.action_feedback = new ControlDroneActionFeedback();
        }

    }
}
