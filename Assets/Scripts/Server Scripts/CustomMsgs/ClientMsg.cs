using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.interface_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

namespace ROSBridgeLib
{

	public class ClientMsg : ROSBridgeMsg
	{

		private int _id;
		private string _name;
		private string _type;
		private TopicTypesMsg[] _topics;
		private TopicTypesMsg[] _services;
			
		public ClientMsg(JSONNode msg)
		{
			_id = int.Parse(msg["_id"]);
			_name = msg["_name"].ToString();
			_type = msg["_type"].ToString();

			JSONArray temp1 = msg["_topics"].AsArray;
			JSONArray temp2 = msg["_services"].AsArray;

			_topics = new TopicTypesMsg[temp1.Count];
			_services = new TopicTypesMsg[temp2.Count];

			for (int i = 0; i < _topics.Length; i++)
            {
				_topics[i] = new TopicTypesMsg(temp1[i]);
            }

			for (int i = 0; i < _services.Length; i++)
			{
				_services[i] = new TopicTypesMsg(temp2[i]);
			}
		}

		public static string getMessageType()
		{
			return "issacs_server/client";
		}

		public int getId()
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

		public TopicTypesMsg[] getTopics()
        {
			return _topics;
        }

		public TopicTypesMsg[] getServices()
        {
			return _services;
        }

		}
	
}