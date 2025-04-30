using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class GenerateCube : MonoBehaviour
{
    [Tooltip("Number of squares along one side of the cube")]
    [SerializeField] private int _divisions = 64;

    [Tooltip("Size of the cube")]
    [SerializeField] private float _size = 2f;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private Vector2[] _uvs;

    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        GetComponent<BoxCollider>().size = new Vector3(_size, _size, _size);

        CreateCube();
    }

    private void CreateCube()
    {
        if (_divisions < 1) _divisions = 1;

        CreateCubeMesh();
    }

    private void CreateCubeMesh()
    {
        int verticesPerFace = (_divisions + 1) * (_divisions + 1);
        int faceCount = 6;

        _vertices = new Vector3[verticesPerFace * faceCount];
        _uvs = new Vector2[_vertices.Length];
        _triangles = new int[_divisions * _divisions * 6 * faceCount];

        int vertexOffset = 0;
        int triangleOffset = 0;

        for (int i = 0; i < faceCount; i++)
        {
            CreateFace(i, ref vertexOffset, ref triangleOffset);
        }

        UpdateMesh();
    }

    private void CreateFace(int faceIndex, ref int vertexOffset, ref int triangleOffset)
    {
        Vector3 normal = Vector3.zero;
        Vector3 right = Vector3.zero;
        Vector3 up = Vector3.zero;

        switch (faceIndex)
        {
            case 0: // Front
                normal = Vector3.forward;
                right = Vector3.right;
                up = Vector3.up;
                break;
            case 1: // Back
                normal = Vector3.back;
                right = Vector3.left;
                up = Vector3.up;
                break;
            case 2: // Left
                normal = Vector3.left;
                right = Vector3.forward;
                up = Vector3.up;
                break;
            case 3: // Right
                normal = Vector3.right;
                right = Vector3.back;
                up = Vector3.up;
                break;
            case 4: // Top
                normal = Vector3.up;
                right = Vector3.right;
                up = Vector3.back;
                break;
            case 5: // Bottom
                normal = Vector3.down;
                right = Vector3.right;
                up = Vector3.forward;
                break;
        }

        for (int y = 0; y <= _divisions; y++)
        {
            for (int x = 0; x <= _divisions; x++)
            {
                int currentVertex = vertexOffset + y * (_divisions + 1) + x;

                Vector3 position = normal * (_size / 2) + (x / (float)_divisions - 0.5f) * _size * right + (y / (float)_divisions - 0.5f) * _size * up;
                _vertices[currentVertex] = position;
                _uvs[currentVertex] = new Vector2(x / (float)_divisions, y / (float)_divisions);

                if (x < _divisions && y < _divisions)
                {
                    int topLeft = currentVertex;
                    int topRight = currentVertex + 1;
                    int bottomLeft = currentVertex + (_divisions + 1);
                    int bottomRight = currentVertex + (_divisions + 1) + 1;

                    _triangles[triangleOffset++] = topLeft;
                    _triangles[triangleOffset++] = topRight;
                    _triangles[triangleOffset++] = bottomLeft;

                    _triangles[triangleOffset++] = bottomRight;
                    _triangles[triangleOffset++] = bottomLeft;
                    _triangles[triangleOffset++] = topRight;
                }
            }
        }

        vertexOffset += (_divisions + 1) * (_divisions + 1);
    }

    void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;

        _mesh.RecalculateNormals();
    }
}
