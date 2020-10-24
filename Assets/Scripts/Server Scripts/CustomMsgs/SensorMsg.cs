using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class SensorMsg : ROSBridgeMsg
	{
		private int _parent_drone_id;
		private ClientMsg _sensor;

		public SensorMsg(JSONNode msg)
		{
			_parent_drone_id = int.Parse(msg["_parent_drone_id"]);
			_sensor = new ClientMsg(msg);
		}

		public static string getMessageType()
		{
			return "isaacs_server/client";
		}

		public int getParentDroneId()
        {
			return _parent_drone_id;
        }

		public ClientMsg getSensor()
		{
			return _sensor;
		}

	}
}
