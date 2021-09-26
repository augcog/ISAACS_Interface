using System.Collections;
using System.Text;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;

/**
 * Define a Matrix4x4 Message
 */

namespace ROSBridgeLib {
	namespace geometry_msgs {
		public class Matrix4x4Msg : ROSBridgeMsg {
			private Vector4[] rows = new Vector4[4];

			public Matrix4x4Msg(JSONNode msg) {
				rows[0] = new Vector4(msg["row1"][0].AsFloat, msg["row1"][1].AsFloat, msg["row1"][2].AsFloat, msg["row1"][3].AsFloat);
				rows[1] = new Vector4(msg["row2"][0].AsFloat, msg["row2"][1].AsFloat, msg["row2"][2].AsFloat, msg["row2"][3].AsFloat);
				rows[2] = new Vector4(msg["row3"][0].AsFloat, msg["row3"][1].AsFloat, msg["row3"][2].AsFloat, msg["row3"][3].AsFloat);
				rows[3] = new Vector4(msg["row4"][0].AsFloat, msg["row4"][1].AsFloat, msg["row4"][2].AsFloat, msg["row4"][3].AsFloat);
			}

			public Matrix4x4Msg(Vector4 row1, Vector4 row2, Vector4 row3, Vector4 row4) {
				rows[0] = row1;
				rows[1] = row2;
				rows[2] = row3;
				rows[3] = row4;
			}

			public Vector4 GetColumn(int col)
			{
				return new Vector4(rows[0][col], rows[1][col], rows[2][col], rows[3][col]);
			}
			
			public static string getMessageType() {
				return "/zed2marker_transform";
			}
			
			public override string ToString() {
				return "/zed2marker_transform {row1=[" + rows[0].ToString() + "],  row2=[" + rows[1].ToString() + "], row3=[" + rows[2].ToString() + "], row4=[" + rows[3].ToString() + "]}";
			}

			private string vectorToYaml(Vector4 v)
			{
				return "[" +"\"" + v.x.ToString() +"\", " 
						   +"\"" + v.y.ToString() +"\", " 
						   +"\"" + v.z.ToString() +"\", " 
						   +"\"" + v.w.ToString() +"\" " 
					+ "]";
			}
					
			public override string ToYAMLString() {
				return "{\"row1\": " + vectorToYaml(rows[0]) + ", \"row2\": " + vectorToYaml(rows[1]) + ", \"row3\": " + vectorToYaml(rows[2]) + ", \"row4\": " + vectorToYaml(rows[3]) + "}";
			}
		}
	}
}
