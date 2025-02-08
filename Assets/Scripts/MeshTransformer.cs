using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(MoveAboveGrid))]
public class MeshTransformer : MonoBehaviour
{
    [Tooltip("The target object to be transformed")]
    public GameObject targetObject;

    [Tooltip("The grid object for reference")]
    public GameObject grid;

    private int mode;
    private float gridSize;
    private float gridHeightOffset;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] previousVertices;
    private Vector3 previousPosition;

    private MoveAboveGrid moveAboveGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ObjectManager objectManager = GetComponentInParent<ObjectManager>();
        if (objectManager == null)
        {
            Debug.LogError("ObjectManager component is missing!");
            return;
        }

        mode = objectManager.mode;
        GameObject targetGrid = objectManager.GetGrid();
        gridSize = targetGrid.GetComponent<Grid>().size * targetGrid.GetComponent<Grid>().squareSize;
        gridHeightOffset = targetGrid.GetComponent<Grid>().heightOffset;

        moveAboveGrid = GetComponent<MoveAboveGrid>();
        if (moveAboveGrid == null)
        {
            Debug.LogError("MoveAboveGrid component is missing!");
            return;
        }

        mesh = new Mesh();

        UpdateShape();
        previousPosition = targetObject.transform.position;

        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetVerticesChanged() || TargetPositionChanged())
        {
            UpdateShape();
        }
    }

    public Mesh GetMesh()
    {
        return mesh;
    }

    void UpdateShape()
    {
        MeshFilter targetMeshFilter = targetObject.GetComponent<MeshFilter>();

        Mesh targetMesh = targetMeshFilter.mesh;
        Vector3[] targetVertices = targetMesh.vertices;
        previousVertices = targetVertices;

        if (grid)
        {
            moveAboveGrid.AdjustHeightAboveGrid(grid.transform.position.y, gridHeightOffset);
        }

        Vector3 targetObjectPosition = targetObject.transform.position;

        if (mode == 1)
        {
            ApplyCircularTransformation(targetVertices, targetMesh, targetObjectPosition);
        }
        else if (mode == 2)
        {
            ApplyStretchTransformation(targetVertices, targetMesh, targetObjectPosition);
        }

        UpdateMesh();
    }

    void ApplyCircularTransformation(Vector3[] targetVertices, Mesh targetMesh, Vector3 targetObjectPosition)
    {
        vertices = new Vector3[targetVertices.Length];

        for (int i = 0; i < targetVertices.Length; i++)
        {
            float targetX = targetVertices[i].x;
            float targetZ = targetVertices[i].z;
            targetX += (targetObjectPosition.x);
            targetZ += (targetObjectPosition.z);

            float gridMin = -gridSize / 2;

            float normalizedX = (targetX - gridMin) / gridSize;
            float normalizedZ = (targetZ - gridMin) / gridSize;

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

    void ApplyStretchTransformation(Vector3[] targetVertices, Mesh targetMesh, Vector3 targetObjectPosition)
    {
        float stretchFactor = 2.0f;
        vertices = new Vector3[targetVertices.Length];

        for (int i = 0; i < targetVertices.Length; i++)
        {
            float targetX = targetVertices[i].x;
            float targetZ = targetVertices[i].z;
            targetX += (targetObjectPosition.x);
            targetZ += (targetObjectPosition.z);

            float gridMin = -gridSize / 2;

            float normalizedX = (targetX - gridMin) / gridSize;
            float normalizedZ = (targetZ - gridMin) / gridSize;

            normalizedX += 0.5f;
            normalizedZ -= 0.5f;

            targetX = normalizedX * 8;
            targetZ = normalizedZ * 8;

            vertices[i] = new Vector3(targetX * stretchFactor + 4, targetVertices[i].y, targetZ);
        }

        triangles = targetMesh.triangles;
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
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

