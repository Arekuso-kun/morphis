using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateGrid : MonoBehaviour
{
    [Tooltip("Number of squares along one side of the grid")]
    public int Size = 8;

    [Tooltip("Size of each square in the grid")]
    public float SquareSize = 1.0f;

    [Tooltip("Number of subdivisions for each square")]
    public int Subdivisions = 16;

    private float _offset;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector2[] _uvs;

    void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        CreateGrid();
    }

    void CreateGrid()
    {
        _offset = Size / 2.0f;
        int verticesPerRow = (Size * Subdivisions) + 1;
        _vertices = new Vector3[verticesPerRow * verticesPerRow];
        _uvs = new Vector2[_vertices.Length];

        // Generate vertices and uvs
        for (int index = 0, y = 0; y <= Size * Subdivisions; y++)
        {
            for (int x = 0; x <= Size * Subdivisions; x++, index++)
            {
                _vertices[index] = new Vector3((x - _offset * Subdivisions) * SquareSize / Subdivisions, 0, (y - _offset * Subdivisions) * SquareSize / Subdivisions);
                _uvs[index] = new Vector2((float)x / (Size * Subdivisions), (float)y / (Size * Subdivisions));
            }
        }

        // Generate triangles
        _triangles = new int[Size * Size * Subdivisions * Subdivisions * 6];
        int triangleIndex = 0;

        for (int y = 0; y < Size * Subdivisions; y++)
        {
            for (int x = 0; x < Size * Subdivisions; x++)
            {
                int vertexIndex = y * verticesPerRow + x;

                if (x / Subdivisions % 2 == y / Subdivisions % 2)
                {
                    _triangles[triangleIndex] = vertexIndex;
                    _triangles[triangleIndex + 1] = vertexIndex + verticesPerRow;
                    _triangles[triangleIndex + 2] = vertexIndex + 1;
                    _triangles[triangleIndex + 3] = vertexIndex + 1;
                    _triangles[triangleIndex + 4] = vertexIndex + verticesPerRow;
                    _triangles[triangleIndex + 5] = vertexIndex + verticesPerRow + 1;

                    triangleIndex += 6;
                }
            }
        }

        UpdateMesh();
    }

    void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;
        _mesh.RecalculateNormals();
    }

    public Vector2 GetDimensions()
    {
        return new Vector2(Size * SquareSize, Size * SquareSize);
    }
}
