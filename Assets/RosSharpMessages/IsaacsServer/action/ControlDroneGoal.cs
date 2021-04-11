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
    public class ControlDroneGoal : Message
    {
        public const string RosMessageName = "isaacs_server/ControlDroneGoal";

        //  Define the goal (parameter)
        public uint id { get; set; }
        public string control_task { get; set; }

        public ControlDroneGoal()
        {
            this.id = 0;
            this.control_task = "";
        }

        public ControlDroneGoal(uint id, string control_task)
        {
            this.id = id;
            this.control_task = control_task;
        }
    }
}
