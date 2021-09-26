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
    public class RegisterSensorResponse : Message
    {
        public const string RosMessageName = "isaacs_server/RegisterSensor";

        public bool success { get; set; }
        public uint id { get; set; }
        public string message { get; set; }

        public RegisterSensorResponse()
        {
            this.success = false;
            this.id = 0;
            this.message = "";
        }

        public RegisterSensorResponse(bool success, uint id, string message)
        {
            this.success = success;
            this.id = id;
            this.message = message;
        }
    }
}
