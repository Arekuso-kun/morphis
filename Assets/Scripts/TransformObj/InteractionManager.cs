using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Tooltip("The grid for the generated object")]
    [SerializeField] private GameObject _grid;

    [Tooltip("The generated object to be assigned to the main object")]
    [SerializeField] private GameObject _generatedObject;

    [Tooltip("List of GameObjects that need to be updated when the object is transformed")]
    public List<GameObject> StatusUpdate = new();

    [Header("Pulse Settings")]
    public float PulseSpeed = 5f;
    public float MinAlpha = 0.85f;
    public float MaxAlpha = 0.95f;

    private MeshTransformer _meshTransformer;
    private GameObject _previewObject;
    private GameObject _mainObject;
    private bool _isHovering = false;
    private Material _previewMaterial;
    private GridSnap _gridSnap;

    private ObjectManager _objectManager;

    void Start()
    {
        if (_generatedObject == null)
        {
            Debug.LogError("Generated object not assigned!");
            enabled = false;
            return;
        }

        _meshTransformer = _generatedObject.GetComponent<MeshTransformer>();
        if (_meshTransformer == null)
        {
            Debug.LogError("MeshTransformer component is missing on Generated Object!");
            enabled = false;
            return;
        }

        _objectManager = GetComponent<ObjectManager>();
        if (_objectManager == null)
        {
            Debug.LogError("ObjectManager is missing!");
            enabled = false;
            return;
        }

        _mainObject = _objectManager.GetObject();
        if (_mainObject == null)
        {
            Debug.LogError("Main object is missing!");
            enabled = false;
            return;
        }

        _gridSnap = _mainObject.GetComponent<GridSnap>();
        if (_gridSnap == null)
        {
            Debug.LogError("GridSnap component not found on Main Object!");
            enabled = false;
            return;
        }

        _previewObject = _objectManager.GetPreview();
        if (_previewObject == null)
        {
            Debug.LogError("Preview object is missing!");
            enabled = false;
            return;
        }
        _previewObject.GetComponent<MeshRenderer>().enabled = false;
        _previewMaterial = _previewObject.GetComponent<MeshRenderer>().material;

        ApplyHoverEffect(false);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform == _generatedObject.transform)
            {
                if (!_isHovering)
                {
                    ApplyHoverEffect(true);
                    ShowPreviewObject();
                }

                if (Input.GetMouseButtonDown(0) && !_gridSnap.IsRotating) // Left click
                {
                    SaveObjectState();
                    ApplyTransformation();
                    HidePreviewObject();

                    foreach (GameObject obj in StatusUpdate)
                        obj.GetComponent<UpdateTrigger>().NeedsUpdate = true;
                }
            }
            else if (_isHovering)
            {
                ApplyHoverEffect(false);
                HidePreviewObject();
            }
        }
        else if (_isHovering)
        {
            ApplyHoverEffect(false);
            HidePreviewObject();
        }

        if (_isHovering && _previewMaterial != null)
        {
            float alpha = Mathf.Lerp(MinAlpha, MaxAlpha, Mathf.Sin(Time.time * PulseSpeed) * 0.5f + 0.5f);
            Color color = _previewMaterial.color;
            color.a = alpha;
            _previewMaterial.color = color;
        }
    }

    private void SaveObjectState()
    {
        ObjectManager objectManager = GetComponentInParent<ObjectManager>();
        if (objectManager == null)
        {
            Debug.LogError("ObjectManager is missing!");
            return;
        }

        GameObject targetObject = objectManager.GetObject();
        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        Mesh currentMesh = meshFilter.mesh;

        BoxCollider boxCollider = targetObject.GetComponent<BoxCollider>();
        Vector3 colliderSize = boxCollider.size;

        Vector3 localPosition = targetObject.transform.localPosition;
        Quaternion rotation = targetObject.transform.rotation;
        int mode = objectManager.Mode;

        HistoryManager historyManager = targetObject.GetComponent<HistoryManager>();
        historyManager.SaveObjectState(currentMesh, localPosition, rotation, colliderSize, mode);
    }

    private Mesh OffsetVertices(Mesh initialMesh)
    {
        Mesh newMesh = Instantiate(initialMesh);

        Vector3 centerOffset = _generatedObject.GetComponent<Renderer>().bounds.center - transform.position;
        centerOffset.y = 0;

        Vector3[] newVertices = initialMesh.vertices;
        for (int i = 0; i < newVertices.Length; i++)
        {
            newVertices[i] -= centerOffset;
        }

        newMesh.vertices = newVertices;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        return newMesh;
    }

    private void ApplyTransformation()
    {
        Mesh generatedMesh = _meshTransformer.GetMesh();

        Mesh newMesh = OffsetVertices(generatedMesh);
        _mainObject.GetComponent<MeshFilter>().mesh = newMesh;

        Vector3 boundsSize = _generatedObject.GetComponent<Renderer>().bounds.size;
        _mainObject.GetComponent<BoxCollider>().size = boundsSize;

        _mainObject.transform.rotation = Quaternion.identity;
    }

    private void ApplyHoverEffect(bool isHovering)
    {
        if (isHovering)
        {
            _generatedObject.layer = LayerMask.NameToLayer("Hover");
        }
        else
        {
            _generatedObject.layer = LayerMask.NameToLayer("Objects");
        }

        this._isHovering = isHovering;
    }

    private void ShowPreviewObject()
    {
        _previewObject.GetComponent<MeshRenderer>().enabled = true;
        _previewObject.GetComponent<BoxCollider>().enabled = true;
        _previewObject.GetComponent<Rigidbody>().isKinematic = false;
        _mainObject.GetComponent<MeshRenderer>().enabled = false;

        Mesh generatedMesh = _meshTransformer.GetMesh();

        Mesh newMesh = OffsetVertices(generatedMesh);
        _previewObject.GetComponent<MeshFilter>().mesh = Instantiate(newMesh);

        Vector3 boundsSize = _generatedObject.GetComponent<Renderer>().bounds.size;
        _previewObject.GetComponent<BoxCollider>().size = boundsSize;

        _previewObject.transform.rotation = Quaternion.identity;
        _previewObject.transform.position = _mainObject.transform.position;
    }

    private void HidePreviewObject()
    {
        _previewObject.GetComponent<MeshRenderer>().enabled = false;
        _previewObject.GetComponent<BoxCollider>().enabled = false;
        _previewObject.GetComponent<Rigidbody>().isKinematic = true;
        _mainObject.GetComponent<MeshRenderer>().enabled = true;
    }
}
