using Unity.VisualScripting;
using UnityEngine;

public class CircularExtension : MonoBehaviour
{
    [Tooltip("The target object to be transformed")]
    public GameObject targetObject;

    [Tooltip("The grid object for reference")]
    public GameObject grid;

    [Tooltip("The mode of transformation")]
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

        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        foreach (var vertex in targetVertices)
        {
            if (vertex.x < minX) minX = vertex.x;
            if (vertex.x > maxX) maxX = vertex.x;
            if (vertex.z < minZ) minZ = vertex.z;
            if (vertex.z > maxZ) maxZ = vertex.z;
        }

        Vector3 gridCenter = Vector3.zero;
        if (grid)
        {
            gridCenter = grid.transform.position;
            float halfHeight = GetComponent<Renderer>().bounds.size.y / 2.0f;
            transform.position = grid.transform.position + new Vector3(0, halfHeight, 0);
        }
        else
        {
            gridCenter = transform.position;
        }
        Vector3 targetObjectPosition = targetObject.transform.position;

        minX += (targetObjectPosition.x - gridCenter.x);
        maxX += (targetObjectPosition.x - gridCenter.x);
        minZ += (targetObjectPosition.z - gridCenter.z);
        maxZ += (targetObjectPosition.z - gridCenter.z);

        float maxAll = 4f;

        minX = -maxAll;
        maxX = maxAll;
        minZ = -maxAll;
        maxZ = maxAll;

        vertices = new Vector3[targetVertices.Length];

        for (int i = 0; i < targetVertices.Length; i++)
        {
            float targetX = targetVertices[i].x;
            targetX += (targetObjectPosition.x - gridCenter.x);
            float targetZ = targetVertices[i].z;
            targetZ += (targetObjectPosition.z - gridCenter.z);

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

            targetZ1 += (targetObjectPosition.z - gridCenter.z);
            targetZ2 += (targetObjectPosition.z - gridCenter.z);
            targetZ3 += (targetObjectPosition.z - gridCenter.z);

            float targetZcenter = (targetZ1 + targetZ2 + targetZ3) / 3;

            if (targetZcenter > -4.5f)
            {
                (triangles[i + 1], triangles[i]) = (triangles[i], triangles[i + 1]);
            }
        }

        UpdateMesh();
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

