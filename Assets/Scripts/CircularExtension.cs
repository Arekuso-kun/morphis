using Unity.VisualScripting;
using UnityEngine;

public class CircularExtension : MonoBehaviour
{
    [Tooltip("The target object to be transformed")]
    public GameObject targetObject;

    [Tooltip("The grid object for reference")]
    public GameObject grid;

    [Tooltip("The mode of transformation (1 = circular, 2 = stretch)")]
    public int mode = 1;

    private Vector3[] previousVertices;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3 previousPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        UpdateShape();
        previousPosition = targetObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetVerticesChanged() || TargetPositionChanged())
        {
            UpdateShape();
        }
    }

    void UpdateShape()
    {
        MeshFilter targetMeshFilter = targetObject.GetComponent<MeshFilter>();

        Mesh targetMesh = targetMeshFilter.mesh;
        Vector3[] targetVertices = targetMesh.vertices;
        previousVertices = targetVertices;

        Vector3 gridCenter = transform.position;
        if (grid)
        {
            gridCenter = grid.transform.position;
            float halfHeight = GetComponent<Renderer>().bounds.size.y / 2.0f;
            transform.position = grid.transform.position + new Vector3(0, halfHeight, 0);
        }

        Vector3 targetObjectPosition = targetObject.transform.position;

        float maxAll = 4f;

        float minX = -maxAll;
        float minZ = -maxAll;
        float maxX = maxAll;
        float maxZ = maxAll;

        if (mode == 1)
        {
            ApplyCircularTransformation(targetVertices, targetMesh, gridCenter, targetObjectPosition, minX, minZ, maxX, maxZ);
        }
        else if (mode == 2)
        {
            ApplyStretchTransformation(targetVertices, targetMesh, gridCenter, targetObjectPosition, minX, minZ, maxX, maxZ);
        }

        UpdateMesh();
    }

    void ApplyCircularTransformation(Vector3[] targetVertices, Mesh targetMesh, Vector3 gridCenter, Vector3 targetObjectPosition, float minX, float minZ, float maxX, float maxZ)
    {
        vertices = new Vector3[targetVertices.Length];

        for (int i = 0; i < targetVertices.Length; i++)
        {
            float targetX = targetVertices[i].x;
            float targetZ = targetVertices[i].z;
            targetX += (targetObjectPosition.x);
            targetZ += (targetObjectPosition.z);

            float normalizedX = (targetX - minX) / (maxX - minX);
            float normalizedZ = (targetZ - minZ) / (maxZ - minZ);

            float angle = normalizedX * Mathf.PI * 2;

            float x = Mathf.Cos(angle) * normalizedZ * 8;
            float z = Mathf.Sin(angle) * normalizedZ * 8;

            vertices[i] = new Vector3(x, targetVertices[i].y, z);
        }

        triangles = targetMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            float targetZ1 = targetVertices[triangles[i]].z;
            float targetZ2 = targetVertices[triangles[i + 1]].z;
            float targetZ3 = targetVertices[triangles[i + 2]].z;

            targetZ1 += (targetObjectPosition.z);
            targetZ2 += (targetObjectPosition.z);
            targetZ3 += (targetObjectPosition.z);

            float targetZcenter = (targetZ1 + targetZ2 + targetZ3) / 3;

            if (targetZcenter > -4.5f)
            {
                (triangles[i + 1], triangles[i]) = (triangles[i], triangles[i + 1]);
            }
        }
    }

    void ApplyStretchTransformation(Vector3[] targetVertices, Mesh targetMesh, Vector3 gridCenter, Vector3 targetObjectPosition, float minX, float minZ, float maxX, float maxZ)
    {
        float stretchFactor = 2.0f;
        vertices = new Vector3[targetVertices.Length];

        for (int i = 0; i < targetVertices.Length; i++)
        {
            float targetX = targetVertices[i].x;
            float targetZ = targetVertices[i].z;
            targetX += (targetObjectPosition.x);
            targetZ += (targetObjectPosition.z);

            float normalizedX = (targetX - minX) / (maxX - minX);
            float normalizedZ = (targetZ - minZ) / (maxZ - minZ);

            normalizedX += 0.5f;
            normalizedZ -= 0.5f;

            targetX = normalizedX * 8;
            targetZ = normalizedZ * 8;

            vertices[i] = new Vector3(targetX * stretchFactor + 4, targetVertices[i].y, targetZ);
        }

        // Debug.Log(targetVertices[1]);

        triangles = targetMesh.triangles;
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    private bool TargetVerticesChanged()
    {
        if (targetObject == null) return false;

        MeshFilter targetMeshFilter = targetObject.GetComponent<MeshFilter>();
        if (targetMeshFilter == null) return false;

        Mesh targetMesh = targetMeshFilter.mesh;
        Vector3[] currentVertices = targetMesh.vertices;

        if (previousVertices == null || previousVertices.Length != currentVertices.Length)
        {
            return true;
        }

        for (int i = 0; i < currentVertices.Length; i++)
        {
            if (previousVertices[i] != currentVertices[i])
            {
                return true;
            }
        }

        return false;
    }

    private bool TargetPositionChanged()
    {
        if (targetObject == null) return false;

        Vector3 currentPosition = targetObject.transform.position;
        if (currentPosition != previousPosition)
        {
            previousPosition = currentPosition;
            return true;
        }
        return false;
    }
}

