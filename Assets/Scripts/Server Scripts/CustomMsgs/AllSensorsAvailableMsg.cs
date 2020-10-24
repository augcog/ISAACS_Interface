using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class AllSensorsAvailableMsg : ROSBridgeMsg
	{
		private SensorMsg[] _sensors_available;

		public AllSensorsAvailableMsg(JSONNode msg)
        {
			JSONArray temp = msg["sensors_available"].AsArray;
			_sensors_available = new SensorMsg[temp.Count];

			for (int i = 0; i < _sensors_available.Length; i++)
            {
				_sensors_available[i] = new SensorMsg(temp[i]);
            }
        }

		public static string getMessageType()
		{
			return "/server/all_sensors_available";

		}

		public SensorMsg[] getSensorsAvailable()
        {
			return _sensors_available;
        }
	}
}