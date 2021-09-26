/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



using RosSharp.RosBridgeClient.MessageTypes.IsaacsServer;

namespace RosSharp.RosBridgeClient.MessageTypes.IsaacsServer
{
    public class AllDronesAvailableResponse : Message
    {
        public const string RosMessageName = "isaacs_server/AllDronesAvailable";

        public Drone[] drones_available { get; set; }
        public string message { get; set; }
        public bool success { get; set; }

        public AllDronesAvailableResponse()
        {
            this.drones_available = new Drone[0];
            this.message = "";
            this.success = false;
        }

        public AllDronesAvailableResponse(Drone[] drones_available, string message, bool success)
        {
            this.drones_available = drones_available;
            this.message = message;
            this.success = success;
        }
    }
}
