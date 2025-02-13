using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MoveAboveGrid))]
public class MeshTransformer : MonoBehaviour
{
    [Tooltip("The grid object for reference")]
    public GameObject grid;

    private GameObject targetObject;
    private GameObject targetGrid;

    private int mode;
    private float gridSize;
    private float gridHeightOffset;

    private Mesh newMesh;
    private Vector3[] newVertices;
    private int[] newTriangles;

    private Vector3[] previousVertices;
    private Vector3 previousPosition;
    private Vector3 previousBounds;

    private MoveAboveGrid moveAboveGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAboveGrid = GetComponent<MoveAboveGrid>();

        ObjectManager objectManager = GetComponentInParent<ObjectManager>();
        if (objectManager == null)
        {
            Debug.LogError("ObjectManager component is missing!");
            return;
        }

        mode = objectManager.mode;
        targetObject = grid ? objectManager.GetObject() : objectManager.GetGrid();
        targetGrid = objectManager.GetGrid();
        if (targetObject == null || targetGrid == null)
        {
            Debug.LogError("Target Object or Target Grid is missing!");
            return;
        }

        Grid gridComponent = targetGrid.GetComponent<Grid>();
        if (gridComponent == null)
        {
            Debug.LogError("Grid component is missing!");
            return;
        }

        gridSize = gridComponent.size * gridComponent.squareSize;
        gridHeightOffset = gridComponent.heightOffset;

        newMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = newMesh;

        previousPosition = targetObject.transform.position;

        UpdateShape();
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetVerticesChanged() || TargetPositionChanged())
        {
            UpdateShape();
        }

        if (BoundsChanged())
        {
            UpdateCollider();
        }
    }

    public Mesh GetMesh()
    {
        return newMesh;
    }

    void UpdateCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        Vector3 boundsSize = GetComponent<Renderer>().bounds.size;
        boundsSize.y = Mathf.Max(boundsSize.y, 0.1f);
        boxCollider.size = boundsSize;

        Vector3 boundsCenter = GetComponent<Renderer>().bounds.center;
        boundsCenter.y = 0;
        Vector3 parentPosition = transform.parent.position;
        boxCollider.center = boundsCenter - parentPosition;
    }

    void UpdateShape()
    {
        MeshFilter targetMeshFilter = targetObject.GetComponent<MeshFilter>();
        previousVertices = targetMeshFilter.mesh.vertices;

        if (grid)
        {
            moveAboveGrid.AdjustHeightAboveGrid(grid.transform.position.y, gridHeightOffset);
        }

        Vector3 targetObjectPosition = targetObject.transform.position;
        Vector3 targetGridPosition = targetGrid.transform.position;

        if (mode == 1)
        {
            ApplyCircularTransformation(targetMeshFilter.mesh, targetObjectPosition, targetGridPosition);
        }
        else if (mode == 2)
        {
            ApplyStretchTransformation(targetMeshFilter.mesh, targetObjectPosition, targetGridPosition, 2.0f);
        }
        else if (mode == 3)
        {
            ApplyStretchTransformation(targetMeshFilter.mesh, targetObjectPosition, targetGridPosition, 0.5f);
        }
        else if (mode == 4)
        {
            ApplyWavyTransformation(targetMeshFilter.mesh, targetObjectPosition, targetGridPosition);
        }

        UpdateMesh();
        UpdateCollider();
    }

    void ApplyCircularTransformation(Mesh targetMesh, Vector3 targetObjectPosition, Vector3 targetGridPosition)
    {
        Vector3[] targetVertices = targetMesh.vertices;
        newVertices = new Vector3[targetVertices.Length];
        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = targetObjectPosition - targetGridPosition;
            Vector3 adjustedVertex = targetVertices[i] + offsetVertex;

            float gridMin = -gridSize / 2;
            float normalizedX = (adjustedVertex.x - gridMin) / gridSize;
            float normalizedZ = (adjustedVertex.z - gridMin) / gridSize;

            float angle = normalizedX * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * normalizedZ * 8;
            float z = Mathf.Sin(angle) * normalizedZ * 8;

            newVertices[i] = new Vector3(x, targetVertices[i].y, z);
        }

        newTriangles = targetMesh.triangles;
        for (int i = 0; i < newTriangles.Length; i += 3)
        {
            float targetZ1 = targetVertices[newTriangles[i]].z + targetObjectPosition.z;
            float targetZ2 = targetVertices[newTriangles[i + 1]].z + targetObjectPosition.z;
            float targetZ3 = targetVertices[newTriangles[i + 2]].z + targetObjectPosition.z;

            float targetZcenter = (targetZ1 + targetZ2 + targetZ3) / 3;

            if (targetZcenter > -4f)
            {
                (newTriangles[i + 1], newTriangles[i]) = (newTriangles[i], newTriangles[i + 1]);
            }
        }
    }

    void ApplyStretchTransformation(Mesh targetMesh, Vector3 targetObjectPosition, Vector3 targetGridPosition, float stretchFactor)
    {
        Vector3[] targetVertices = targetMesh.vertices;
        newVertices = new Vector3[targetVertices.Length];
        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = targetObjectPosition - targetGridPosition;
            Vector3 adjustedVertex = targetVertices[i] + offsetVertex;

            newVertices[i] = new Vector3(adjustedVertex.x * stretchFactor, targetVertices[i].y, adjustedVertex.z);
        }

        newTriangles = targetMesh.triangles;
    }

    void ApplyWavyTransformation(Mesh targetMesh, Vector3 targetObjectPosition, Vector3 targetGridPosition)
    {
        Vector3[] targetVertices = targetMesh.vertices;
        newVertices = new Vector3[targetVertices.Length];

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = targetObjectPosition - targetGridPosition;
            Vector3 adjustedVertex = targetVertices[i] + offsetVertex;

            float gridMin = -gridSize / 2;
            float normalizedX = (adjustedVertex.x - gridMin) / gridSize;

            float waveHeight = Mathf.Sin(normalizedX * Mathf.PI * 2) * 0.5f;

            newVertices[i] = new Vector3(adjustedVertex.x, targetVertices[i].y, adjustedVertex.z + waveHeight);
        }

        newTriangles = targetMesh.triangles;
    }

    void UpdateMesh()
    {
        newMesh.Clear();

        newMesh.vertices = newVertices;
        newMesh.triangles = newTriangles;

        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
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

    private bool BoundsChanged()
    {
        if (targetObject == null) return false;

        Vector3 currentBounds = targetObject.GetComponent<Renderer>().bounds.size;
        if (currentBounds != previousBounds)
        {
            previousBounds = currentBounds;
            return true;
        }
        return false;
    }
}

