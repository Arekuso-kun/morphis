using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class GeneratePlane : MonoBehaviour
{
    [Tooltip("Number of squares along one edge of the plane")]
    [SerializeField] private int _divisions = 64;

    [Tooltip("Size of the plane")]
    [SerializeField] private float _size = 8f;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector2[] _uvs;

    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        GetComponent<BoxCollider>().size = new Vector3(_size, _size, 0.001f);

        CreatePlane();
    }

    private void CreatePlane()
    {
        if (_divisions < 1) _divisions = 1;

        int vertPerSide = _divisions + 1;
        int vertCount = vertPerSide * vertPerSide;
        int triCount = _divisions * _divisions * 6;

        _vertices = new Vector3[vertCount * 2];
        _uvs = new Vector2[vertCount * 2];
        _triangles = new int[triCount * 2];

        float divisionSize = _size / _divisions;
        float gap = 0.001f;
        int triangleOffset = 0;

        // generate vertices and uvs
        for (int y = 0; y < vertPerSide; y++)
        {
            for (int x = 0; x < vertPerSide; x++)
            {
                int index = y * vertPerSide + x;

                _vertices[index] = new Vector3(-_size / 2 + x * divisionSize, -_size / 2 + y * divisionSize, gap);
                _uvs[index] = new Vector2(x / (float)_divisions, y / (float)_divisions);

                _vertices[index + vertCount] = new Vector3(-_size / 2 + x * divisionSize, -_size / 2 + y * divisionSize, -gap);
                _uvs[index + vertCount] = new Vector2(x / (float)_divisions, y / (float)_divisions);
            }
        }

        // generate triangles
        for (int z = 0; z < _divisions; z++)
        {
            for (int x = 0; x < _divisions; x++)
            {
                int topLeft = z * vertPerSide + x;
                int bottomLeft = (z + 1) * vertPerSide + x;
                int topRight = topLeft + 1;
                int bottomRight = bottomLeft + 1;

                // front
                _triangles[triangleOffset++] = topLeft;
                _triangles[triangleOffset++] = bottomRight;
                _triangles[triangleOffset++] = bottomLeft;

                _triangles[triangleOffset++] = topLeft;
                _triangles[triangleOffset++] = topRight;
                _triangles[triangleOffset++] = bottomRight;

                // back
                _triangles[triangleOffset++] = bottomLeft + vertCount;
                _triangles[triangleOffset++] = bottomRight + vertCount;
                _triangles[triangleOffset++] = topLeft + vertCount;

                _triangles[triangleOffset++] = bottomRight + vertCount;
                _triangles[triangleOffset++] = topRight + vertCount;
                _triangles[triangleOffset++] = topLeft + vertCount;
            }
        }

        UpdateMesh();

        Debug.Log($"Generated plane with {_vertices.Length} vertices and {_triangles.Length / 3} triangles.");
    }

    private void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;
        _mesh.RecalculateNormals();
    }
}
