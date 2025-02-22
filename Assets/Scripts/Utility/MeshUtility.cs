using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MeshUtility
{
    public static void SaveMeshToFile(string fileName, MeshFilter meshFilter)
    {
#if UNITY_EDITOR
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
#endif
    }
}
