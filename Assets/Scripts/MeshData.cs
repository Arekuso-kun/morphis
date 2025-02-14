using UnityEngine;

[System.Serializable]
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;

    public MeshData(Mesh mesh)
    {
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        uv = mesh.uv;
    }
}