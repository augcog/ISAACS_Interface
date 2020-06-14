using UnityEngine;
using SimpleJSON;
using System;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace ROSBridgeLib
{
    namespace voxblox_msgs
    {
        public class MeshBlockMsg: ROSBridgeMsg
        {
            /// <summary>
            /// Index of meshed points in block map
            /// </summary>
            Int64[] _index;

            /// <summary>
            /// Triangle Information (always in groups of 3)
            /// </summary>
            UInt16[] _x, _y, _z;

            /// <summary>
            /// Color information
            /// Note that some information may be missing.
            /// </summary>
            byte[] _r, _g, _b;

            public Int64[] GetIndex()
            {
                //return (Int64[])_index.Clone();
                return _index;
            }

            public UInt16[] GetX()
            {
                //return (UInt16[])_x.Clone();
                return _x;
            }

            public UInt16[] GetY()
            {
                //return (UInt16[])_y.Clone();
                return _y;
            }

            public UInt16[] GetZ()
            {
                //return (UInt16[])_z.Clone();
                return _z;
            }

            public byte[] GetR()
            {
                //return (byte[])_r.Clone();
                return _r;
            }

            public byte[] GetG()
            {
                //return (byte[])_g.Clone();
                return _g;
            }

            public byte[] GetB()
            {
                //return (byte[])_b.Clone();
                return _b;
            }

            /// <summary>
            /// Returns MeshBlock from JSON
            /// </summary>
            /// <param name="msg"></param>
            public MeshBlockMsg(JSONNode msg)
            {
                DateTime startTime = DateTime.Now;
                DateTime stageTime = startTime;
                JSONArray temp = msg["index"].AsArray;
                _index = new Int64[3];
                for (int i = 0; i < _index.Length; i++)
                {
                    // TODO get an int64 instead of an int32.
                    _index[i] = temp[i].AsInt;
                }
                Debug.Log("Index Time: " + DateTime.Now.Subtract(stageTime).TotalMilliseconds.ToString() + "ms");
                stageTime = DateTime.Now;
                JSONArray tempX = msg["x"].AsArray;
                JSONArray tempY = msg["y"].AsArray;
                JSONArray tempZ = msg["z"].AsArray;
                _x = new UInt16[tempX.Count];
                _y = new UInt16[tempY.Count];
                _z = new UInt16[tempZ.Count];
                for (int i = 0; i < _x.Length; i++)
                {
                    _x[i] = (UInt16)tempX[i].AsInt;
                    _y[i] = (UInt16)tempY[i].AsInt;
                    _z[i] = (UInt16)tempZ[i].AsInt;
                }
                
                Debug.Log("Position Time: " + DateTime.Now.Subtract(stageTime).TotalMilliseconds.ToString() + "ms");
                stageTime = DateTime.Now;
               
                String tempColor = msg["r"].Value;
                _r = Convert.FromBase64String(tempColor);
                tempColor = msg["b"].Value;
                _b = Convert.FromBase64String(tempColor);
                tempColor = msg["g"].Value;
                _g = Convert.FromBase64String(tempColor);
                
                Debug.Log("Color Time: " + DateTime.Now.Subtract(stageTime).TotalMilliseconds.ToString() + "ms");
                Debug.Log("Total Time: " + DateTime.Now.Subtract(startTime).TotalMilliseconds.ToString() + "ms");
            }

            public static string getMessageType()
            {
                return "voxblox_msgs/MeshBlock";
            }

        }
    }
}