using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class QueryTopicsMsg : ROSBridgeMsg
	{
		private int _id;
		private TopicTypesMsg[] _all_topics;

		public QueryTopicsMsg(JSONNode msg)
        {
            // Confirm this logic with Server side
            // Key problem we want to address is that we shouldn't have all_topics be associated with one drone/sensor
            if (msg["_id"].ToString() != null)
            {
                _id = int.Parse(msg["_id"]);
            }
            else
            {
                _id = -1;
            }


            JSONArray temp = msg["_all_topics"].AsArray;
            _all_topics = new TopicTypesMsg[temp.Count];

            for (int i = 0; i < _all_topics.Length; i++)
            {
                _all_topics[i] = new TopicTypesMsg(temp[i]);
            }

        }

		public static string getMessageType()
		{
			return "/server/query_topics";
		}

        public int getId()
        {
            return _id;
        }

        public TopicTypesMsg[] getAllTopics()
        {
            return _all_topics;
        }
	}
}