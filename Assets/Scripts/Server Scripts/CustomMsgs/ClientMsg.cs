using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class ClientMsg : ROSBridgeLib
	{

		private int32 _id;
		private string _name;
		private string _type;
		private TopicTypesMsg[] _topics;
		private TopicTypesMsg[] _services;
			
		public ClientMsg(JSONNode msg)
		{
			_id = Int32.Parse(msg["_id"]);
			_name = msg["_name"].ToString();
			_type = msg["_type"].ToString();

			JSONArray temp1 = msg["_topics"].AsArray;
			JSONArray temp2 = msg["_services"].AsArray;

			_topics = new TopicTypesMsg[temp1.Count];
			_services = new TopicTypesMsg[temp2.Count];

			for (int i = 0; i < _topics.length; i++)
            {
				_topics[i] = new TopicTypesMsg(temp1[i]);
            }

			for (int i = 0; i < _services.length; i++)
			{
				_services[i] = new TopicTypesMsg(temp2[i]);
			}
		}

		public static string getMessageType()
		{
			return "issacs_server/client";
		}

		public int32 getId()
        {
			return _id;
        }

		public string getName()
        {
			return _name;
        }

		public string getType()
        {
			return _type;
        }

		public TopicsTypeMsg[] getTopics()
        {
			return _topics;
        }

		public TopicsTypeMsg[] getServices()
        {
			return _services;
        }

		}
	
}