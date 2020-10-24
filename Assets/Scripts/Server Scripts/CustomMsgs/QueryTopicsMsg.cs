using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class QueryTopicsMsg : ROSBridgeLib
	{
		private int _id;
		private TopicTypesMsg[] _all_topics;

		public QueryTopicsMsg(JSONNode msg)
        {
			if (msg["_id"].ToString() != null)
            {
				_id = int.Parse(msg["_id"]);
				_all_topics = new DroneMsg(msg).getDrone().getTopics();
			}

			// ...
        }

		public static string getMessageType()
		{
			return "/server/query_topics";
		}
	}
}