using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(UpdateTrigger))]
public class MeshTransformer : MonoBehaviour
{
    [Tooltip("The grid object for reference")]
    [SerializeField] private GameObject _grid;

    [Header("Materials")]
    [SerializeField] private Material _invalidMaterial;
    [SerializeField] private Material _validMaterial;

    private GameObject _targetObject;
    private GameObject _targetGrid;

    private TransformationMode _mode;
    private float _gridSize;

    private Mesh _newMesh;
    private Vector3[] _newVertices;
    private int[] _newTriangles;
    private Vector2[] _newUVs;

    private Vector3 _previousPosition;
    private Quaternion _previousRotation;
    private Vector3 _previousBounds;

    private ObjectManager _objectManager;

    void Start()
    {
        _objectManager = GetComponentInParent<ObjectManager>();
        if (_objectManager == null)
        {
            Debug.LogError("ObjectManager component not found in parent!");
            enabled = false;
            return;
        }

        _mode = _objectManager.Mode;
        _targetObject = _grid ? _objectManager.GetObject() : _objectManager.GetGrid();
        if (_targetObject == null)
        {
            Debug.LogError("Target Object is missing!");
            enabled = false;
            return;
        }

        if (_targetObject.GetComponent<MeshFilter>() == null)
        {
            Debug.LogError("MeshFilter component not found on target object!");
            enabled = false;
            return;
        }

        if (_targetObject.GetComponent<Renderer>() == null)
        {
            Debug.LogError("Renderer component not found on target object!");
            enabled = false;
            return;
        }

        if (_targetObject.GetComponent<GridSnap>() == null)
        {
            Debug.LogWarning("GridSnap component not found on target object!");
        }

        _targetGrid = _objectManager.GetGrid();
        if (_targetGrid == null)
        {
            Debug.LogError("Target Grid is missing!");
            enabled = false;
            return;
        }

        GenerateGrid gridComponent = _targetGrid.GetComponent<GenerateGrid>();
        if (gridComponent == null)
        {
            Debug.LogError("GenerateGrid component not found on target grid!");
            enabled = false;
            return;
        }

        _gridSize = gridComponent.Size * gridComponent.SquareSize;

        _newMesh = new Mesh { name = "Mesh" };
        GetComponent<MeshFilter>().mesh = _newMesh;

        _previousPosition = _targetObject.transform.position;
        _previousRotation = _targetObject.transform.rotation;

        UpdateShape();
    }

    void Update()
    {
        if (_grid && _invalidMaterial && _validMaterial && _targetObject.GetComponent<GridSnap>())
        {
            if (_targetObject.GetComponent<GridSnap>().IsOutOfBounds)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<BoxCollider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;
                _grid.GetComponent<MeshRenderer>().material = _invalidMaterial;
                // Debug.Log("Out of bounds!");
            }
            else
            {
                GetComponent<MeshRenderer>().enabled = true;
                GetComponent<BoxCollider>().enabled = true;
                GetComponent<Rigidbody>().isKinematic = false;
                _grid.GetComponent<MeshRenderer>().material = _validMaterial;
                // Debug.Log("In bounds!");
            }
        }

        if (_mode != _objectManager.Mode)
        {
            _mode = _objectManager.Mode;
            UpdateShape();
            UpdateCollider();
        }

        if (GetComponent<UpdateTrigger>().NeedsUpdate || TargetTransformChanged() || BoundsChanged())
        {
            UpdateShape();
            UpdateCollider();
            GetComponent<UpdateTrigger>().NeedsUpdate = false;
        }
    }

    public Mesh GetMesh()
    {
        return _newMesh;
    }

    void UpdateCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        Vector3 boundsSize = GetComponent<Renderer>().bounds.size;
        boundsSize.y = Mathf.Max(boundsSize.y, 0.01f);
        boxCollider.size = boundsSize;

        Vector3 boundsCenter = GetComponent<Renderer>().bounds.center;
        boundsCenter.y = 0;
        Vector3 parentPosition = transform.parent.position;
        boxCollider.center = boundsCenter - parentPosition;
    }

    void UpdateShape()
    {
        MeshFilter targetMeshFilter = _targetObject.GetComponent<MeshFilter>();

        Transform targetObjectTransform = _targetObject.transform;
        Transform targetGridTransform = _targetGrid.transform;

        switch (_mode)
        {
            case TransformationMode.Circular:
                ApplyCircularTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, false);
                break;
            case TransformationMode.CircularSquared:
                ApplyCircularTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, true);
                break;
            case TransformationMode.Stretch:
                ApplyStretchTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, 2.0f);
                break;
            case TransformationMode.Shrink:
                ApplyStretchTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, 0.5f);
                break;
            case TransformationMode.Wavy:
                ApplyWavyTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, false, 4);
                break;
            case TransformationMode.WavySharp:
                ApplyWavyTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, true, 4);
                break;
            case TransformationMode.Shear:
                ApplyShearTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, 0.5f);
                break;
            case TransformationMode.Expand:
                ApplyStretchTransformation(targetMeshFilter.mesh, targetObjectTransform, targetGridTransform, 1.0f, 2.0f);
                break;
            default:
                Debug.LogError("Invalid transformation mode selected!");
                return;
        }

        UpdateMesh();
        UpdateCollider();
    }

    void ApplyCircularTransformation(Mesh targetMesh, Transform targetObjectTransform, Transform targetGridTransform, bool square)
    {
        Vector3 targetObjectPosition = targetObjectTransform.position;
        Vector3 targetGridPosition = targetGridTransform.position;

        Vector3[] targetVertices = targetMesh.vertices;
        _newVertices = new Vector3[targetVertices.Length];

        Vector3[] transformedVertices = new Vector3[targetVertices.Length];
        targetObjectTransform.TransformPoints(targetVertices, transformedVertices);

        Vector3 offsetVertex = new(targetGridPosition.x, targetObjectPosition.y, targetGridPosition.z);
        float gridMin = -_gridSize / 2;

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 adjustedVertex = transformedVertices[i] - offsetVertex;

            float normalizedX = (adjustedVertex.x - gridMin) / _gridSize;
            float normalizedZ = (adjustedVertex.z - gridMin) / _gridSize;

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

            x *= normalizedZ * _gridSize;
            z *= normalizedZ * _gridSize;

            _newVertices[i] = new Vector3(x, adjustedVertex.y, z);
        }

        _newTriangles = targetMesh.triangles;

        for (int i = 0; i < _newTriangles.Length; i += 3)
        {
            Vector3 adjustedVertex1 = transformedVertices[_newTriangles[i]] - offsetVertex;
            Vector3 adjustedVertex2 = transformedVertices[_newTriangles[i + 1]] - offsetVertex;
            Vector3 adjustedVertex3 = transformedVertices[_newTriangles[i + 2]] - offsetVertex;

            float triangleCenterZ = (adjustedVertex1.z + adjustedVertex2.z + adjustedVertex3.z) / 3;

            if (triangleCenterZ > -_gridSize / 2)
            {
                (_newTriangles[i + 1], _newTriangles[i]) = (_newTriangles[i], _newTriangles[i + 1]);
            }

            // TO DO: Fix vertices near the center of the circle
        }

        _newUVs = targetMesh.uv;
    }

    void ApplyStretchTransformation(Mesh targetMesh, Transform targetObjectTransform, Transform targetGridTransform, float stretchFactor, float expandFactor = 1.0f)
    {
        Vector3 targetObjectPosition = targetObjectTransform.position;
        Vector3 targetGridPosition = targetGridTransform.position;

        Vector3[] targetVertices = targetMesh.vertices;
        _newVertices = new Vector3[targetVertices.Length];

        Vector3[] transformedPoints = new Vector3[targetVertices.Length];
        targetObjectTransform.TransformPoints(targetVertices, transformedPoints);

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = new(targetGridPosition.x, targetObjectPosition.y, targetGridPosition.z);
            Vector3 adjustedVertex = transformedPoints[i] - offsetVertex;

            _newVertices[i] = new Vector3(adjustedVertex.x * stretchFactor * expandFactor, adjustedVertex.y * expandFactor, adjustedVertex.z * expandFactor);
        }

        _newTriangles = targetMesh.triangles;
        _newUVs = targetMesh.uv;
    }

    void ApplyWavyTransformation(Mesh targetMesh, Transform targetObjectTransform, Transform targetGridTransform, bool sharp, int waveCount)
    {
        Vector3 targetObjectPosition = targetObjectTransform.position;
        Vector3 targetGridPosition = targetGridTransform.position;

        Vector3[] targetVertices = targetMesh.vertices;
        _newVertices = new Vector3[targetVertices.Length];

        Vector3[] transformedPoints = new Vector3[targetVertices.Length];
        targetObjectTransform.TransformPoints(targetVertices, transformedPoints);

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = new(targetGridPosition.x, targetObjectPosition.y, targetGridPosition.z);
            Vector3 adjustedVertex = transformedPoints[i] - offsetVertex;

            float normalizedX = adjustedVertex.x / _gridSize;

            float waveFactor = normalizedX * Mathf.PI * waveCount;
            float waveHeight = (sharp ? SharpSin(waveFactor) : Mathf.Sin(waveFactor)) / 2;

            _newVertices[i] = new Vector3(adjustedVertex.x, adjustedVertex.y, adjustedVertex.z + waveHeight);
        }

        _newTriangles = targetMesh.triangles;
        _newUVs = targetMesh.uv;
    }

    void ApplyShearTransformation(Mesh targetMesh, Transform targetObjectTransform, Transform targetGridTransform, float shear)
    {
        Vector3 targetObjectPosition = targetObjectTransform.position;
        Vector3 targetGridPosition = targetGridTransform.position;

        Vector3[] targetVertices = targetMesh.vertices;
        _newVertices = new Vector3[targetVertices.Length];

        Vector3[] transformedPoints = new Vector3[targetVertices.Length];
        targetObjectTransform.TransformPoints(targetVertices, transformedPoints);

        for (int i = 0; i < targetVertices.Length; i++)
        {
            Vector3 offsetVertex = new(targetGridPosition.x, targetObjectPosition.y, targetGridPosition.z);
            Vector3 adjustedVertex = transformedPoints[i] - offsetVertex;

            _newVertices[i] = new Vector3(adjustedVertex.x + shear * adjustedVertex.z, adjustedVertex.y, adjustedVertex.z);
        }

        _newTriangles = targetMesh.triangles;
        _newUVs = targetMesh.uv;
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
        _newMesh.Clear();

        _newMesh.vertices = _newVertices;
        _newMesh.triangles = _newTriangles;
        _newMesh.uv = _newUVs;

        _newMesh.RecalculateNormals();
        _newMesh.RecalculateBounds();
    }

    private bool TargetTransformChanged()
    {
        if (_targetObject == null) return false;

        Vector3 currentPosition = _targetObject.transform.position;
        Quaternion currentRotation = _targetObject.transform.rotation;

        if (currentPosition != _previousPosition || currentRotation != _previousRotation)
        {
            _previousPosition = currentPosition;
            _previousRotation = currentRotation;
            return true;
        }
        return false;
    }

    private bool BoundsChanged()
    {
        if (_targetObject == null) return false;

        Vector3 currentBounds = _targetObject.GetComponent<Renderer>().bounds.size;
        if (currentBounds != _previousBounds)
        {
            _previousBounds = currentBounds;
            return true;
        }
        return false;
    }
}

