using System;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib.voxblox_msgs;

public class MeshVisualizer : MonoBehaviour
{
    /// <value> Attach DataServer object. If nonexistant, create an empty GameObject and attach the script `DataServer.cs`.</value>
    public bool flipYZ = false;

    /// <summary>
    /// Object that holds all the individual mesh blocks.
    /// </summary>
    private GameObject meshParent;

    private bool hasChanged = false;

    /// <summary>
    /// Number of faces required for a block to be deemed worthy of being rendered.
    /// </summary>
    public int faceThreshold = 25;
    /// <summary>
    /// Time in seconds between accepting updates of a block.
    /// </summary>
    public float updateInterval = 30.0f;
    /// <summary>
    /// Maximum distance from the drone to override update time delay. Note that this is total taxi distance x+y+z not crow distance x^2+y^2+z^2
    /// </summary>
    public float distThreshold = 10.0f;

    /// <summary>
    /// Dictionary of meshes for each index.
    /// </summary>
    private Dictionary<Int64[], MeshFilter> mesh_dict;    // Use this for initialization

    /// <summary>
    /// Dictionary of last update times for each index.
    /// </summary>
    private Dictionary<Int64[], float> last_update;
    void Start()
    {
        mesh_dict = new Dictionary<long[], MeshFilter>(new LongArrayEqualityComparer());
        last_update = new Dictionary<long[], float>(new LongArrayEqualityComparer());
        meshParent = new GameObject("Mesh");
    }

    // Update is called once per frame
    void Update()
    {
        if (hasChanged)
        {
            // Do stuff maybe?
        }
    }

    /// <summary>
    /// Returns if a block is close enough to the drone to warrant updating.
    /// </summary>
    /// <param name="index">Location of the block.</param>
    /// <returns>If the block is within distThreshold of the drone</returns>
    private bool closeToDrone(Int64[] index)
    {
        // TODO take into account blocklength. Right now we are assuming a block length of 1 which is incorrect.
        Int64[] dronePosition = new Int64[3] { 0, 0, 0 };
        long dist = Math.Abs(index[0] - dronePosition[0]) + Math.Abs(index[1] - dronePosition[1]) + Math.Abs(index[2] - dronePosition[2]);
        return dist < distThreshold;
    }

    /// <summary>
    /// Returns if a block should update. Either due to not being updated in a while or being close to the drone.
    /// </summary>
    /// <param name="index">Location of the block</param>
    /// <returns>If the block should update</returns>
    private bool shouldUpdate(Int64[] index)
    {
        if (!last_update.ContainsKey(index))
        {
            return true;
        }
        return closeToDrone(index) || last_update[index] < Time.time - updateInterval;
    }

    /// <summary>
    /// Update the mesh with the new mesh.
    /// </summary>
    /// <param name="meshMsg">ROSBridge Voxblox Mesh Message</param>
    public void SetMesh(MeshMsg meshMsg)
    {
        Debug.Log("Setting New Mesh");
        /// The length of one block. Also the scaling factor of the coordinates.
        float scale_factor = meshMsg.GetBlockEdgeLength();
        /// List of all the mesh blocks.
        MeshBlockMsg[] mesh_blocks = meshMsg.GetMeshBlocks();
        Debug.Log(mesh_blocks.Length);
        /// Iterate through each mesh block generating and updating meshes for each.
        for (int i = 0; i < mesh_blocks.Length; i++)
        {
            /// index of the mesh block.
            Int64[] index = mesh_blocks[i].GetIndex();
            
            if (!shouldUpdate(index))
            {
                Debug.Log("Delay Update");
                continue;
            }
            
            ushort[] x = mesh_blocks[i].GetX();

            // If there is no existing game object for the block, create one.
            if (!mesh_dict.ContainsKey(index))
            {
                // only render meshes with enough faces to make it worth the resources of an extra game object
                if (x.Length < faceThreshold)
                {
                    Debug.Log("Not enough faces");
                    continue;
                }

                GameObject meshObject = new GameObject(index.ToString());
                meshObject.transform.parent = meshParent.transform;
                MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
                //meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
                mesh_dict.Add(index, meshFilter);
            }
            else
            {
                Debug.Log("Reusing GameObject");
            }

       
            ushort[] y = mesh_blocks[i].GetY();
            ushort[] z = mesh_blocks[i].GetZ();
            
            // Create a list of vertices and their corresponding colors.
            List<Vector3> newVertices = new List<Vector3>();
            List<Color> newColors = new List<Color>();
        
            // update indicies, converting from block index to global position transforms.
            for (int j = 0; j < x.Length; j++)
            {
                float zv = ((float)z[j] / 32768.0f + index[2]) * scale_factor;
                float xv = ((float)x[j] / 32768.0f + index[0]) * scale_factor;
                float yv = ((float)y[j] / 32768.0f + index[1]) * scale_factor;
                if (flipYZ)
                {
                    newVertices.Add(new Vector3(xv, zv, yv));
                } 
                else
                {
                    newVertices.Add(new Vector3(xv, yv, zv));
                }
            }
            // update colors
            byte[] r = mesh_blocks[i].GetR();
            byte[] g = mesh_blocks[i].GetG();
            byte[] b = mesh_blocks[i].GetB();

            for (int j = 0; j < r.Length; j++)
            {
                newColors.Add(new Color32(r[j], g[j], b[j], 51));
            }

            // Vertices come in triples each corresponding to one face.
            int[] newTriangles = new int[newVertices.Count / 3 * 3];
            for (int j = 0; j < newTriangles.Length; j++)
            {
                newTriangles[j] = j;
            }

            Mesh mesh = new Mesh();
                       // correct for inverted mesh. By reversing the lists, the normal vectors point the right direction.
            newVertices.Reverse();
            newColors.Reverse();

            mesh.vertices = newVertices.ToArray();
            //mesh.uv = newUV;
            mesh.triangles = newTriangles;
            mesh.colors = newColors.ToArray();
            mesh_dict[index].mesh = mesh;
            last_update[index] = Time.time;
        }
        hasChanged = true;
    }
}

/// <summary>
/// Element wise equality comparer for long arrays. This is primarily used for updating the dictionary using the block index.
/// </summary>
public class LongArrayEqualityComparer : IEqualityComparer<long[]>
{
    public bool Equals(long[] x, long[] y)
    {
        if (x.Length != y.Length)
        {
            return false;
        }
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
            {
                return false;
            }
        }
        return true;
    }

    public int GetHashCode(long[] obj)
    {
        int result = 17;
        for (int i = 0; i < obj.Length; i++)
        {
            unchecked
            {
                result = (int)(((long)result * 23) + obj[i]);
            }
        }
        return result;
    }
}
