using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using System;

namespace ROSBridgeLib
{

	public class DroneCommandMsg : ROSBridgeLib
	{
		private int _drone_id;
		private bool _success;
		private string _meta_data;

		private DroneMsg drone;

		public DroneCommandMsg(JSONNode msg)
        {
			_drone_id = int.Parse(msg["_drone_id"]);
			_success = Convert.ToBoolean(msg["_sucess"]);
			_meta_data = msg["_meta_data"].ToString();

			DroneMsg[] drones_available = new AllDronesAvailableMsg(msg).getDronesAvailable();
			for (int i = 0; i < drones_available.Length; i++)
			{
				if (drones_available[i].getDrone().getId() == _drone_id)
				{
					drone = drones_available[i];
					break;
				}
			}

			// Call StartMission method in drone instance
			// Add drone_id to appropriate callback and return

		}

		public static string getMessageType()
		{
			return "/server/drone_command";
		}

		public DroneMsg getDrone()
        {
			return drone;
        }
	}
}