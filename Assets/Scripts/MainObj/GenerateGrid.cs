using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateGrid : MonoBehaviour
{
    [Tooltip("Number of squares along one side of the grid")]
    public int size = 8;

    [Tooltip("Size of each square in the grid")]
    public float squareSize = 1.0f;

    [Tooltip("Height offset for the grid vertices")]
    public float heightOffset = 0.01f;

    [Tooltip("Number of subdivisions for each square")]
    public int subdivisions = 16;

    private float offset;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateGrid();
    }

    void CreateGrid()
    {
        offset = size / 2.0f;
        int verticesPerRow = (size * subdivisions) + 1;
        vertices = new Vector3[verticesPerRow * verticesPerRow];
        uvs = new Vector2[vertices.Length];

        // Generate vertices and uvs
        for (int index = 0, y = 0; y <= size * subdivisions; y++)
        {
            for (int x = 0; x <= size * subdivisions; x++, index++)
            {
                vertices[index] = new Vector3((x - offset * subdivisions) * squareSize / subdivisions, heightOffset, (y - offset * subdivisions) * squareSize / subdivisions);
                uvs[index] = new Vector2((float)x / (size * subdivisions), (float)y / (size * subdivisions));
            }
        }

        // Generate triangles
        triangles = new int[size * size * subdivisions * subdivisions * 6];
        int triangleIndex = 0;

        for (int y = 0; y < size * subdivisions; y++)
        {
            for (int x = 0; x < size * subdivisions; x++)
            {
                int vertexIndex = y * verticesPerRow + x;

                if (x / subdivisions % 2 == y / subdivisions % 2)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + verticesPerRow;
                    triangles[triangleIndex + 2] = vertexIndex + 1;
                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = vertexIndex + verticesPerRow;
                    triangles[triangleIndex + 5] = vertexIndex + verticesPerRow + 1;

                    triangleIndex += 6;
                }
            }
        }

        UpdateMesh();
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    public Vector2 GetDimensions()
    {
        return new Vector2(size * squareSize, size * squareSize);
    }

    void Update()
    {
    }
}
