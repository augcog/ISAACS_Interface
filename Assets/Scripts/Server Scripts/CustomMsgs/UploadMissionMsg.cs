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

		public UploadMissionMsg(JSONNode msg)
        {
            msg = msg["values"];
            _drone_id = int.Parse(msg["id"]);
			_success = Convert.ToBoolean(msg["success"]);
			_meta_data = msg["message"].ToString();
		}

		public static string getMessageType()
		{
			return "/server/upload_mission";
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