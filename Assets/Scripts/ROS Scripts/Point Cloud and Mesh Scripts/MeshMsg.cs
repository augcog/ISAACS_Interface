using UnityEngine;
using SimpleJSON;
using System;
using System.Runtime.CompilerServices;

namespace ROSBridgeLib
{
    namespace voxblox_msgs
    {
        public class MeshMsg: ROSBridgeMsg
        {
            std_msgs.HeaderMsg _header;
            float _block_edge_length;
            voxblox_msgs.MeshBlockMsg[] _mesh_blocks;

            public MeshMsg(JSONNode msg)
            {
                DateTime startTime = DateTime.Now;
                DateTime stageTime = startTime;
                _header = new std_msgs.HeaderMsg(msg["header"]);
//                Debug.Log("Header Generation: " + DateTime.Now.Subtract(stageTime).TotalMilliseconds.ToString() + "ms");
                stageTime = DateTime.Now;
                _block_edge_length = float.Parse(msg["block_edge_length"]);
//                Debug.Log("Block Edge Length Generation: " + DateTime.Now.Subtract(stageTime).TotalMilliseconds.ToString() + "ms");
                stageTime = DateTime.Now;
                JSONArray temp = msg["mesh_blocks"].AsArray;
//                Debug.Log("Mesh Block Array Generation: " + DateTime.Now.Subtract(stageTime).TotalMilliseconds.ToString() + "ms");
                stageTime = DateTime.Now;
                _mesh_blocks = new voxblox_msgs.MeshBlockMsg[temp.Count];
                for (int i = 0; i < _mesh_blocks.Length; i++)
                {
                    _mesh_blocks[i] = new voxblox_msgs.MeshBlockMsg(temp[i]);
                }
                Debug.Log("Mesh Block Generation Total: " + DateTime.Now.Subtract(stageTime).TotalMilliseconds.ToString() + "ms");
                Debug.Log("Mesh Block Generation Average: " + (DateTime.Now.Subtract(stageTime).TotalMilliseconds / _mesh_blocks.Length).ToString() + "ms");
            }

            public static string getMessageType()
            {
                return "voxblox_msgs/Mesh";
            }

            public std_msgs.HeaderMsg GetHeader()
            {
                return _header;
            }

            public float GetBlockEdgeLength()
            {
                return _block_edge_length;
            }

            public voxblox_msgs.MeshBlockMsg[] GetMeshBlocks()
            {
                //return (voxblox_msgs.MeshBlockMsg[])_mesh_blocks.Clone();
                return _mesh_blocks;
            }
        }
    }
}