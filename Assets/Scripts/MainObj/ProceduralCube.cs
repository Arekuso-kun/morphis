using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class ProceduralCube : MonoBehaviour
{
    [Tooltip("Number of squares along one side of the cube")]
    public int divisions = 96;

    [Tooltip("Size of the cube")]
    public float size = 1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        GetComponent<BoxCollider>().size = new Vector3(size, size, size);

        CreateCube();
    }

    private void CreateCube()
    {
        if (divisions < 1) divisions = 1;

        CreateCubeMesh();
    }

    private void CreateCubeMesh()
    {
        int verticesPerFace = (divisions + 1) * (divisions + 1);
        int faceCount = 6;

        vertices = new Vector3[verticesPerFace * faceCount];
        uvs = new Vector2[vertices.Length];
        triangles = new int[divisions * divisions * 6 * faceCount];

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

        for (int y = 0; y <= divisions; y++)
        {
            for (int x = 0; x <= divisions; x++)
            {
                int currentVertex = vertexOffset + y * (divisions + 1) + x;

                Vector3 position = normal * (size / 2) + (x / (float)divisions - 0.5f) * size * right + (y / (float)divisions - 0.5f) * size * up;
                vertices[currentVertex] = position;
                uvs[currentVertex] = new Vector2(x / (float)divisions, y / (float)divisions);

                if (x < divisions && y < divisions)
                {
                    int topLeft = currentVertex;
                    int topRight = currentVertex + 1;
                    int bottomLeft = currentVertex + (divisions + 1);
                    int bottomRight = currentVertex + (divisions + 1) + 1;

                    triangles[triangleOffset++] = topLeft;
                    triangles[triangleOffset++] = topRight;
                    triangles[triangleOffset++] = bottomLeft;

                    triangles[triangleOffset++] = bottomRight;
                    triangles[triangleOffset++] = bottomLeft;
                    triangles[triangleOffset++] = topRight;
                }
            }
        }

        vertexOffset += (divisions + 1) * (divisions + 1);
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }
}
