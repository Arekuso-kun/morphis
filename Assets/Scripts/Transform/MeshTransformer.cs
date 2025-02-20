using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class MeshTransformer : MonoBehaviour
{
    [Tooltip("The grid object for reference")]
    public GameObject grid;

    private GameObject targetObject;
    private GameObject targetGrid;

    private int mode;
    private float gridSize;

    private Mesh newMesh;
    private Vector3[] newVertices;
    private int[] newTriangles;

    private Vector3[] previousVertices;
    private Vector3 previousPosition;
    private Vector3 previousBounds;

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
        targetObject = grid ? objectManager.GetObject() : objectManager.GetGrid();
        targetGrid = objectManager.GetGrid();
        if (targetObject == null || targetGrid == null)
        {
            Debug.LogError("Target Object or Target Grid is missing!");
            return;
        }

        GenerateGrid gridComponent = targetGrid.GetComponent<GenerateGrid>();
        if (gridComponent == null)
        {
            Debug.LogError("Grid component is missing!");
            return;
        }

        gridSize = gridComponent.size * gridComponent.squareSize;

        newMesh = new Mesh();
        newMesh.name = "Mesh";
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
        boundsSize.y = Mathf.Max(boundsSize.y, 0.0625f);
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

        Transform targetObjectTransform = targetObject.transform;
        Transform targetGridTransform = targetGrid.transform;

        if (mode == 1)
        {
            ApplyCircularTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, false);
        }
        else if (mode == 2)
        {
            ApplyStretchTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, 2.0f);
        }
        else if (mode == 3)
        {
            ApplyStretchTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, 0.5f);
        }
        else if (mode == 4)
        {
            ApplyWavyTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, false, 4);
        }

        UpdateMesh();
        UpdateCollider();
    }

    void ApplyCircularTransformation(Mesh targetMesh, Transform targetObjectTransform, Transform targetGridTransform, bool square)
    {
        Vector3 targetObjectPosition = targetObjectTransform.position;
        Vector3 targetGridPosition = targetGridTransform.position;

        Vector3[] targetVertices = targetMesh.vertices;
        newVertices = new Vector3[targetVertices.Length];

        Vector3[] transformedVertices = new Vector3[targetVertices.Length];
        targetObject.transform.TransformPoints(targetVertices, transformedVertices);

        Vector3 offsetVertex = new(targetGridPosition.x, targetObjectPosition.y, targetGridPosition.z);
        float gridMin = -gridSize / 2;

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 adjustedVertex = transformedVertices[i] - offsetVertex;

            float normalizedX = (adjustedVertex.x - gridMin) / gridSize;
            float normalizedZ = (adjustedVertex.z - gridMin) / gridSize;

            float angle = normalizedX * Mathf.PI * 2;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            float squareX;
            float squareZ;

            if (square)
            {
                if (Mathf.Abs(x) > Mathf.Abs(z))
                {
                    squareX = Mathf.Sqrt(z * z + x * x) * Mathf.Sign(x);
                    squareZ = Mathf.Sqrt(z * z + z * z) * Mathf.Sign(z);
                }
                else
                {
                    squareX = Mathf.Sqrt(x * x + x * x) * Mathf.Sign(x);
                    squareZ = Mathf.Sqrt(x * x + z * z) * Mathf.Sign(z);
                }

                x = squareX;
                z = squareZ;
            }

            x *= normalizedZ * gridSize;
            z *= normalizedZ * gridSize;

            newVertices[i] = new Vector3(x, adjustedVertex.y, z);
        }

        newTriangles = targetMesh.triangles;

        for (int i = 0; i < newTriangles.Length; i += 3)
        {
            Vector3 adjustedVertex1 = transformedVertices[newTriangles[i]] - offsetVertex;
            Vector3 adjustedVertex2 = transformedVertices[newTriangles[i + 1]] - offsetVertex;
            Vector3 adjustedVertex3 = transformedVertices[newTriangles[i + 2]] - offsetVertex;

            float triangleCenterZ = (adjustedVertex1.z + adjustedVertex2.z + adjustedVertex3.z) / 3;

            if (triangleCenterZ > -gridSize / 2)
            {
                (newTriangles[i + 1], newTriangles[i]) = (newTriangles[i], newTriangles[i + 1]);
            }

            // TO DO: Fix vertices near the center of the circle
        }
    }

    void ApplyStretchTransformation(Mesh targetMesh, Transform targetObjectTransform, Transform targetGridTransform, float stretchFactor)
    {
        Vector3 targetObjectPosition = targetObjectTransform.position;
        Vector3 targetGridPosition = targetGridTransform.position;

        Vector3[] targetVertices = targetMesh.vertices;
        newVertices = new Vector3[targetVertices.Length];

        Vector3[] transformedPoints = new Vector3[targetVertices.Length];
        targetObject.transform.TransformPoints(targetVertices, transformedPoints);

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = new(targetGridPosition.x, targetObjectPosition.y, targetGridPosition.z);
            Vector3 adjustedVertex = transformedPoints[i] - offsetVertex;

            newVertices[i] = new Vector3(adjustedVertex.x * stretchFactor, adjustedVertex.y, adjustedVertex.z);
        }

        newTriangles = targetMesh.triangles;
    }

    void ApplyWavyTransformation(Mesh targetMesh, Transform targetObjectTransform, Transform targetGridTransform, bool sharp, int waveCount)
    {
        Vector3 targetObjectPosition = targetObjectTransform.position;
        Vector3 targetGridPosition = targetGridTransform.position;

        Vector3[] targetVertices = targetMesh.vertices;
        newVertices = new Vector3[targetVertices.Length];

        Vector3[] transformedPoints = new Vector3[targetVertices.Length];
        targetObject.transform.TransformPoints(targetVertices, transformedPoints);

        float gridMin = -gridSize / 2;

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = new(targetGridPosition.x, targetObjectPosition.y, targetGridPosition.z);
            Vector3 adjustedVertex = transformedPoints[i] - offsetVertex;

            float normalizedX = (adjustedVertex.x - gridMin) / gridSize;

            float waveFactor = normalizedX * Mathf.PI * waveCount;
            float waveHeight = (sharp ? SharpSin(waveFactor) : Mathf.Sin(waveFactor)) / waveCount;

            newVertices[i] = new Vector3(adjustedVertex.x, adjustedVertex.y, adjustedVertex.z + waveHeight);
        }

        newTriangles = targetMesh.triangles;
    }

    float SharpSin(float x)
    {
        return Mathf.Abs(Mod(x, 2 * Mathf.PI) - Mathf.PI) - (Mathf.PI / 2);
    }

    float Mod(float a, float b)
    {
        return a - b * Mathf.Floor(a / b);
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

