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
		bool _success;

		public DroneCommandMsg(JSONNode msg)
		{
			msg = msg["values"];
			_id = int.Parse(msg["id"]);
			_command = msg["control_task"].ToString();
			_success = msg["success"].AsBool;
		}

		public DroneCommandMsg(String thing)
        {
			_id = 0;
			_command = "sample";
        }

		public static string getMessageType()
		{
			return "/isaacs_server/drone_command";
		}

		public string getCommand()
		{
			return _command;
		}

		public int getID()
        {
			return _id;
        }

		public bool getSuccess()
        {
			return _success;
        }


		public void setCommand(string command)
        {
			_command = command;
        }

		public void setID(int id)
        {
			_id = id;
        }

		public string ToString()
        {
			return string.Format("{0}", _command);

		}
	}
}