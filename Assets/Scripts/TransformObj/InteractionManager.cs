using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Tooltip("The grid for the generated object")]
    public GameObject Grid;

    [Tooltip("The generated object to be assigned to the main object")]
    public GameObject GeneratedObject;

    [Tooltip("List of GameObjects that need to be updated when the object is transformed")]
    public List<GameObject> StatusUpdate = new();

    public TextMeshProUGUI TransformationNameText;
    public TextMeshProUGUI TransformationDescriptionText;
    public GameObject HoverInfoContainer;

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

    private Dictionary<TransformationMode, (string name, string description)> _transformationDescriptions = new()
    {
        { TransformationMode.Circular, ("Circular", "Transforms X axis into the angle from the center and the Z axis into the distance from the center.") },
        { TransformationMode.CircularSquared, ("Circular Squared", "Transforms X axis into the angle from the center and the Z axis into the distance from the center, expanded as a square.") },
        { TransformationMode.Stretch, ("Stretch", "Stretches on the X axis.") },
        { TransformationMode.Shrink, ("Shrink", "Compresses on the X axis.") },
        { TransformationMode.Wavy, ("Wavy", "Adds smooth wave distortions.") },
        { TransformationMode.WavySharp, ("Wavy Sharp", "Adds sharp-edged wave distortions.") },
        { TransformationMode.Shear, ("Shear", "Tilts sideways.") },
        { TransformationMode.Expand, ("Expand", "Expands in all directions.") }
    };


    void Start()
    {
        if (GeneratedObject == null)
        {
            Debug.LogError("Generated object not assigned!");
            enabled = false;
            return;
        }

        _meshTransformer = GeneratedObject.GetComponent<MeshTransformer>();
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
        if (CameraController.GlobalInteractionLock) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform == GeneratedObject.transform)
            {
                if (!_isHovering)
                {
                    ApplyHoverEffect(true);
                }

                if (Input.GetMouseButtonDown(0) && !_gridSnap.IsRotating) // Left click
                {
                    SaveObjectState();
                    ApplyTransformation();

                    foreach (GameObject obj in StatusUpdate)
                        obj.GetComponent<UpdateTrigger>().NeedsUpdate = true;
                }
            }
            else if (_isHovering)
            {
                ApplyHoverEffect(false);
            }
        }
        else if (_isHovering)
        {
            ApplyHoverEffect(false);
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
        TransformationMode mode = objectManager.Mode;

        HistoryManager historyManager = targetObject.GetComponent<HistoryManager>();
        historyManager.SaveObjectState(currentMesh, localPosition, rotation, colliderSize, mode);
    }

    private Mesh OffsetVertices(Mesh initialMesh)
    {
        Mesh newMesh = Instantiate(initialMesh);

        Vector3 centerOffset = GeneratedObject.GetComponent<Renderer>().bounds.center - transform.position;
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

        Vector3 boundsSize = GeneratedObject.GetComponent<Renderer>().bounds.size;
        _mainObject.GetComponent<BoxCollider>().size = boundsSize;

        _mainObject.transform.rotation = Quaternion.identity;

        _mainObject.transform.position = GeneratedObject.GetComponent<Renderer>().bounds.center;

        _mainObject.GetComponent<GridSnap>().IsOutOfBounds = true;
        _mainObject.GetComponent<GridSnap>().IsDragging = true;
    }

    private void ApplyHoverEffect(bool isHovering)
    {
        GeneratedObject.layer = LayerMask.NameToLayer(isHovering ? "Hover" : "Objects");
        this._isHovering = isHovering;

        if (HoverInfoContainer != null)
        {
            HoverInfoContainer.SetActive(isHovering);
        }

        if (isHovering && TransformationNameText != null && TransformationDescriptionText != null)
        {
            var mode = _objectManager.Mode;
            if (_transformationDescriptions.TryGetValue(mode, out var info))
            {
                TransformationNameText.text = info.name;
                TransformationDescriptionText.text = info.description;
            }
        }
    }

}
