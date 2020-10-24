using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using System;

namespace ROSBridgeLib
{

	public class UploadMissionMsg : ROSBridgeMsg
	{
		private int _drone_id;
		private bool _success;
		private string _meta_data;

		private DroneMsg drone;

		public UploadMissionMsg(JSONNode msg)
        {
			_drone_id = int.Parse(msg["_drone_id"]);
			_success = Convert.ToBoolean(msg["_sucess"]);
			_meta_data = msg["_meta_data"];

			DroneMsg[] temp = new AllDronesAvailableMsg(msg).getDronesAvailable();
			for (int i = 0; i < temp.Length; i++)
            {
				if (temp[i].getDrone().getId() == _drone_id)
                {
					drone = temp[i];
					break;
				}
            }

			// Call UploadMission method in drone instance
			// Add drone_id to UploadMissionCallback and return
		}

		public static string getMessageType()
		{
			return "/server/upload_mission";
		}

		public DroneMsg getDrone()
        {
			return drone;
        }
	}
}