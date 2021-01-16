using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using System;

namespace ROSBridgeLib
{

	public class DroneCommandMsg : ROSBridgeMsg
	{
		int _id;
		string _command;

		public DroneCommandMsg(JSONNode msg)
		{
			_id = int.Parse(msg["id"]);
			_meta_data = msg["control_task"].ToString();
		}

		public static string getMessageType()
		{
			return "/server/drone_command";
		}

		public int getCommand()
		{
			return _command;
		}

		public int getID()
        {
			return _id;
        }

		public string toString()
        {
			return string.Format("Latitude: {0}", _command);

		}
	}
}