using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class DroneMsg : ROSBridgeLib
	{
		private ClientMsg _drone;

		public DroneMsg(JSONNode msg)
        {
			_drone = new ClientMsg(msg);
        }

		public static string getMessageType()
		{
			return "issacs_server/drone";
		}

		public ClientMsg getDrone()
        {
			return _drone;
        }

	}		
}
