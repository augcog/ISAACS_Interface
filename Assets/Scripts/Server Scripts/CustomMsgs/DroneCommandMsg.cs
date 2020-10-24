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
		private int _drone_id;
		private bool _success;
		private string _meta_data;

		public DroneCommandMsg(JSONNode msg)
        {
			_drone_id = int.Parse(msg["_drone_id"]);
			_success = Convert.ToBoolean(msg["_sucess"]);
			_meta_data = msg["_meta_data"].ToString();
		}

		public static string getMessageType()
		{
			return "/server/drone_command";
		}

		public int getDroneId()
        {
			return _drone_id;
        }

        public bool getSuccess()
        {
            return _success;
        }

        public string getMetaData()
        {
            return _meta_data;
        }
	}
}