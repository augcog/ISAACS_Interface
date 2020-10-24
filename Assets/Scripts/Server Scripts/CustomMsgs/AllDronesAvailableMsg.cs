using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class AllDronesAvailableMsg : ROSBridgeMsg
	{

		private DroneMsg[] _drones_available;

		public AllDronesAvailableMsg(JSONNode msg)
        {
			JSONArray temp = msg["drones_available"].AsArray;
			_drones_available = new DroneMsg[temp.Count];

			for (int i = 0; i < _drones_available.Length; i++)
            {
				_drones_available[i] = new DroneMsg(temp[i]);
            }
		}

		public static string getMessageType()
        {
			return "/server/all_drones_available";

		}

		public DroneMsg[] getDronesAvailable()
        {
			return _drones_available;
        }

	}
}
