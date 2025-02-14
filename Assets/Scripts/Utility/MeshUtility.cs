using UnityEngine;
using System.IO;

public static class MeshUtility
{
    public static void SaveMeshToFile(string filePath, MeshFilter meshFilter)
    {
        if (meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("No MeshFilter or Mesh found on this object!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        MeshData meshData = new MeshData(mesh);

        string json = JsonUtility.ToJson(meshData);
        File.WriteAllText(filePath, json);

        Debug.Log("Mesh saved to " + filePath);
    }

    public static void LoadMeshFromFile(string filePath, MeshFilter targetMeshFilter)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Saved mesh file not found at: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        MeshData meshData = JsonUtility.FromJson<MeshData>(json);

        Mesh mesh = new Mesh
        {
            vertices = meshData.vertices,
            triangles = meshData.triangles,
            uv = meshData.uv
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        targetMeshFilter.mesh = mesh;
        Debug.Log("Mesh loaded from " + filePath);
    }
}
