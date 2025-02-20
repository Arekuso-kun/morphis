using UnityEngine;
using UnityEditor;

public static class MeshUtility
{
    public static void SaveMeshToFile(string fileName, MeshFilter meshFilter)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilter is null or has no mesh.");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;

        string assetPath = $"Assets/Meshes/{fileName}.asset";

        AssetDatabase.CreateAsset(Object.Instantiate(mesh), assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Mesh saved to: {assetPath}");
    }

    public static void LoadMeshFromFile(string fileName, MeshFilter targetMeshFilter)
    {
        if (targetMeshFilter == null)
        {
            Debug.LogError("Target MeshFilter is null.");
            return;
        }

        string assetPath = $"Assets/Meshes/{fileName}.asset";
        Mesh loadedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

        if (loadedMesh == null)
        {
            Debug.LogError($"Failed to load mesh from {assetPath}");
            return;
        }

        targetMeshFilter.mesh = loadedMesh;
        Debug.Log($"Mesh loaded from: {assetPath}");
    }
}
