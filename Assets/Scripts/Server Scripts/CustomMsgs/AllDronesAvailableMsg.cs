using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using System;

namespace ROSBridgeLib
{

	public class AllDronesAvailableMsg : ROSBridgeMsg
	{
		private DroneMsg[] _drones_available;
		private bool _success;

		public AllDronesAvailableMsg(JSONNode msg)
        {
			msg = msg["values"];
			JSONArray temp = msg["drones_available"].AsArray;
			_success = msg["success"].AsBool;
			Debug.Log(_success);
			_drones_available = new DroneMsg[temp.Count];

			for (int i = 0; i < _drones_available.Length; i++)
            {
				_drones_available[i] = new DroneMsg(temp[i]);
            }
		}

		public static string getMessageType()
        {
			return "/isaacs_server/all_drones_available";

		}

		public bool getSuccess()
        {
			return _success;
        }

		public DroneMsg[] getDronesAvailable()
        {
			return _drones_available;
        }
	}
}
