using SimpleJSON;
using System.Text;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace interface_msgs
    {
        public class SearchMissionTaskMsg : ROSBridgeMsg
        {
            double _lat;
            double _long;
            float _radius;

            public SearchMissionTaskMsg(double latitude, double longitude, float radius)
            {
                _lat = latitude;
                _long = longitude;
                _radius = radius;
            }

            public SearchMissionTaskMsg(JSONNode msg)
            {
                _lat = msg["latitude"].AsFloat;
                _long = msg["longitude"].AsFloat;
                _radius = msg["radius"].AsFloat;
            }

            public static string GetMessageType()
            {
                return "TODO: Apollo";
            }

            public double GetLatitude()
            {
                return _lat;
            }

            public double GetLongitude()
            {
                return _long;
            }

            public float GetRadius()
            {
                return _radius;
            }

            public override string ToYAMLString()
            {
                StringBuilder sb = new StringBuilder("{");
                sb.AppendFormat("\"latitude\": {0}, ", _lat);
                sb.AppendFormat("\"longitude\": {0}, ", _long);
                sb.AppendFormat("\"radius\": {0}, ", _radius);
                sb.Append("}");
                return sb.ToString();
            }
        }
    }
}
