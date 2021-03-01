using System;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib.voxblox_msgs;

public class MeshVisualizer : MonoBehaviour
{
    /// <value> Attach DataServer object. If nonexistant, create an empty GameObject and attach the script `DataServer.cs`.</value>
    public bool flipYZ = true;

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
    /// Alpha value for individual vertex colorings.
    /// </summary>
    public byte alpha = 150;

    private Material material;

    /// <summary>
    /// Dictionary of gameobjects for each index.
    /// </summary>
    private Dictionary<Int64[], GameObject> gameobject_dict;    // Use this for initialization

    /// <summary>
    /// Dictionary of last update times for each index.
    /// </summary>
    private Dictionary<Int64[], float> last_update;

    /// <summary>
    /// Shader to use to render meshes.
    /// </summary>
    [SerializeField] private Shader shader;
    /// <summary>
    /// Color to render the meshes.
    /// </summary>
    public Color color = new Color32(255, 255, 255, 100);

    void Awake()
    {
        shader = Shader.Find("Mobile/Particles/Alpha Blended");
        material = new Material(shader);
    }

    // Update is called once per frame
    void Update()
    {
        SetShader(shader);
        SetColor(color);
    }

    /// <summary>
    /// Instantiate required components for the Mesh and make child of Mesh Sensor
    /// </summary>
    public void CreateMeshVisualizer()
    {
        gameobject_dict = new Dictionary<long[], GameObject>(new LongArrayEqualityComparer());
        last_update = new Dictionary<long[], float>(new LongArrayEqualityComparer());
        meshParent = this.gameObject;
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
    /// Generates a dictionary of MeshArrays specified by the message.
    /// </summary>
    /// <param name="meshMsg">Voxblox Mesh message to generate Meshes with.</param>
    /// <returns>A dictionary of MeshArrays.</returns>
    public Dictionary<long[], MeshArray> generateMesh(MeshMsg meshMsg)
    {
        Dictionary<Int64[], MeshArray> generated_mesh_dict = new Dictionary<long[], MeshArray>(new LongArrayEqualityComparer());
        /// The length of one block. Also the scaling factor of the coordinates.
        float scale_factor = meshMsg.GetBlockEdgeLength();
        /// List of all the mesh blocks.
        MeshBlockMsg[] mesh_blocks = meshMsg.GetMeshBlocks();
        /// Iterate through each mesh block generating and updating meshes for each.
        for (int i = 0; i < mesh_blocks.Length; i++)
        {
            /// index of the mesh block.
            Int64[] index = mesh_blocks[i].GetIndex();

            ushort[] x = mesh_blocks[i].GetX();
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
                newColors.Add(new Color32(r[j], g[j], b[j], alpha));
            }

            // Vertices come in triples each corresponding to one face.
            int[] newTriangles = new int[newVertices.Count / 3 * 3];
            for (int j = 0; j < newTriangles.Length; j++)
            {
                newTriangles[j] = j;
            }

            // correct for inverted mesh. By reversing the lists, the normal vectors point the right direction.
            newVertices.Reverse();
            newColors.Reverse();

            generated_mesh_dict[index] = new MeshArray(newVertices.ToArray(), newTriangles, newColors.ToArray());
        }
        return generated_mesh_dict;
    }

    public void SetMesh(Dictionary<long[], MeshArray> mesh_dict)
    {
        foreach(KeyValuePair<long[], MeshArray> entry in mesh_dict)
        {
            long[] index = entry.Key;
            MeshArray meshArray = entry.Value;
            Mesh mesh = meshArray.GetMesh();
            // If there is no existing game object for the block, create one.
            if (!gameobject_dict.ContainsKey(index))
            {
                GameObject meshObject = new GameObject(index.ToString());
                meshObject.transform.parent = meshParent.transform;
                meshObject.transform.localPosition = new Vector3(0, 0, 0);
                meshObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                meshObject.transform.localScale = new Vector3(1, 1, 1);
                MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
                //meshRenderer.sharedMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
                meshRenderer.sharedMaterial = material;
                gameobject_dict.Add(index, meshObject);
            }
            gameobject_dict[index].GetComponent<MeshFilter>().mesh = mesh;
        }
    }

    /// <summary>
    /// Sets the shader.
    /// </summary>
    /// <param name="shader"></param>
    public void SetShader(Shader shader)
    {
        this.shader = shader;
        material.shader = shader;
    }

    /// <summary>
    /// Sets the Color.
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        material.color = color;
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

/// <summary>
/// A Struct of Arrays necessary to create a Mesh.
/// </summary>
public struct MeshArray
{
    /// <summary>
    /// Create a new Mesh struct.
    /// </summary>
    /// <param name="vertices">Vertices of the mesh (their positions)</param>
    /// <param name="triangles">Triangles each vertex corresponds to</param>
    /// <param name="colors">Colors of each vertex</param>
    public MeshArray(Vector3[] vertices, int[] triangles, Color[] colors)
    {
        Vertices = vertices;
        Triangles = triangles;
        Colors = colors;
    }

    /// <summary>
    /// Vertices of the Mesh.
    /// </summary>
    public Vector3[] Vertices { get; }
    /// <summary>
    /// Triangles each vertex correspond to.
    /// </summary>
    public int[] Triangles { get; }
    /// <summary>
    /// Color of each vertex.
    /// </summary>
    public Color[] Colors { get; }

    /// <summary>
    /// Generate a Mesh using the arrays of this struct. Must be called in the Main thread.
    /// </summary>
    /// <returns>A new mesh</returns>
    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.colors = Colors;
        return mesh;
    }
}
