using SimpleJSON;

namespace ROSBridgeLib
{
    namespace cartographer_msgs
    {
        public class StatusResponseMsg : ROSBridgeMsg
        {
        	public int code;
        	public string message;

        	public StatusResponseMsg(JSONNode msg) {
        		this.code = msg["code"].AsInt;
        		this.message = msg["message"];
        	}

        	public StatusResponseMsg(int code, string message) {
        		this.code = code;
        		this.message = message;
        	}
        }
    }
}