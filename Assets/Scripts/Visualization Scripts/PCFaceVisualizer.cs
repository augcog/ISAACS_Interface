using System;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib.voxblox_msgs;
using ROSBridgeLib.rntools;

public class PCFaceVisualizer : MonoBehaviour
{
    /// <value> Attach DataServer object. If nonexistant, create an empty GameObject and attach the script `DataServer.cs`.</value>
    public bool flipYZ = true;

    /// <summary>
    /// Object that holds all the individual mesh blocks.
    /// </summary>
    private GameObject meshParent;

    private bool hasChanged = false;

    // Update is called once per frame
    void Update()
    {
        if (hasChanged)
        {
            // Do stuff maybe?
        }
    }

    /// <summary>
    /// Set the parent of this visualizer to the sensor
    /// </summary>
    /// <param name="parent"></param>
    public void CreateMeshGameobject(Transform parent)
    {
        meshParent = new GameObject("PCFace Mesh");
        MeshFilter meshFilter = meshParent.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshParent.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        meshParent.transform.parent = parent;
    }

    /// <summary>
    /// Sets the mesh to be of the specified color. Also sets the Shader to Standard.
    /// </summary>
    /// <param name="color">Color to set the mesh to.</param>
    public void SetColor(Color color)
    {
        MeshRenderer meshRenderer = meshParent.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard Unlit"));
        meshRenderer.sharedMaterial.color = color;
    }

    /// <summary>
    /// Update the mesh with the new mesh.
    /// </summary>
    /// <param name="meshMsg">ROSBridge PCFace Mesh Message</param>
    public void SetMesh(PCFaceMsg meshMsg)
    {
        Debug.Log("Setting New PCFace Mesh");

        List<Vector3> newVertices = new List<Vector3>();
        // Also not sure what to do with all the newColors...
        List<Color> newColors = new List<Color>();
        List<int> newTriangles = new List<int>();

        float[] x = meshMsg.Vert_x;
        float[] y = meshMsg.Vert_y;
        float[] z = meshMsg.Vert_z;
        byte[] r = meshMsg.Color_r;
        byte[] g = meshMsg.Color_g;
        byte[] b = meshMsg.Color_b;
        byte[] a = meshMsg.Color_a;
        ushort[] face_0 = meshMsg.Face_0;
        ushort[] face_1 = meshMsg.Face_1;
        ushort[] face_2 = meshMsg.Face_2;
        // Create a list of Vertices and their colors.
        for (int j = 0; j < x.Length; j++)
        {
            if (flipYZ)
            {
                newVertices.Add(new Vector3(x[j], z[j], y[j]));
            } 
            else
            {
                newVertices.Add(new Vector3(x[j], y[j], z[j]));
            }
            newColors.Add(new Color(r[j], g[j], b[j], a[j]));
        }        
        Vector3[] vertices = newVertices.ToArray();
        Color[] colors = newColors.ToArray();

        // Create a list of mesh triangles from the faces arrays. There are as many faces as the length of the face array.
        int[] triangles = new int[face_0.Length * 3];
        for (int j = 0; j < face_0.Length; j++)
        {
            triangles[3 * j] = (int) face_0[j];
            triangles[3 * j + 1] = (int) face_1[j];
            triangles[3 * j + 2] = (int) face_2[j];
        }

        Debug.Log("Face_0 Length: " + meshMsg.Face_0.Length + "Face_1 Length: " + meshMsg.Face_1.Length + "Face_2 Length: " + meshMsg.Face_2.Length);
        Debug.Log("Num Verticies: " + meshMsg.Vert_x.Length);
        Debug.Log("Num Colors: " + meshMsg.Color_a.Length);

        // Generate the Mesh
        Mesh mesh = new Mesh(); 
        mesh.vertices = vertices;
        //mesh.uv = newUV;
        mesh.triangles = triangles;
        mesh.colors = colors;
        
        meshParent.GetComponent<MeshFilter>().mesh = mesh;
        hasChanged = true;
    }
}