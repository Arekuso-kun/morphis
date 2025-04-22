using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Tooltip("The generated object to be assigned to the main object")]
    public GameObject generatedObject;

    [Tooltip("Default material without emission")]
    public Material defaultMaterial;

    [Tooltip("Material with emission for hover effect")]
    public Material emissionMaterial;

    public readonly float pulseSpeed = 5f;
    public readonly float minAlpha = 0.85f;
    public readonly float maxAlpha = 0.95f;

    private MeshTransformer meshTransformer;
    private GameObject previewObject;
    private GameObject mainObject;
    private bool isHovering = false;
    private Material previewMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (generatedObject == null)
        {
            Debug.LogError("Generated object is missing!");
            return;
        }

        meshTransformer = generatedObject.GetComponent<MeshTransformer>();
        if (meshTransformer == null)
        {
            Debug.LogError("MeshTransformer component is missing on Generated Object!");
            return;
        }

        ObjectManager objectManager = GetComponentInParent<ObjectManager>();
        if (objectManager == null)
        {
            Debug.LogError("ObjectManager is missing!");
            return;
        }

        mainObject = objectManager.GetObject();
        if (mainObject == null)
        {
            Debug.LogError("Main object is missing!");
            return;
        }

        previewObject = objectManager.GetPreview();
        if (previewObject == null)
        {
            Debug.LogError("Preview object is missing!");
        }
        else
        {
            previewObject.GetComponent<MeshRenderer>().enabled = false;
            previewMaterial = previewObject.GetComponent<MeshRenderer>().material;
        }

        ApplyHoverEffect(false);
    }

    void Update()
    {
        if (generatedObject == null || meshTransformer == null || previewObject == null || mainObject == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform == generatedObject.transform)
            {
                if (!isHovering)
                {
                    ApplyHoverEffect(true);
                    ShowPreviewObject();
                }

                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    SaveObjectState();
                    ApplyTransformation();
                    HidePreviewObject();
                }
            }
            else if (isHovering)
            {
                ApplyHoverEffect(false);
                HidePreviewObject();
            }
        }
        else if (isHovering)
        {
            ApplyHoverEffect(false);
            HidePreviewObject();
        }

        if (isHovering && previewMaterial != null)
        {
            // float alpha = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * pulseAlpha;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f);
            Color color = previewMaterial.color;
            color.a = alpha;
            previewMaterial.color = color;
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

        UndoManager undoManager = targetObject.GetComponent<UndoManager>();
        undoManager.SaveObjectState(currentMesh, colliderSize);
    }

    private Mesh OffsetVertices(Mesh initialMesh)
    {
        Mesh newMesh = Instantiate(initialMesh);

        Vector3 centerOffset = generatedObject.GetComponent<Renderer>().bounds.center - transform.position;
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
        Mesh generatedMesh = meshTransformer.GetMesh();

        Mesh newMesh = OffsetVertices(generatedMesh);
        mainObject.GetComponent<MeshFilter>().mesh = newMesh;

        Vector3 boundsSize = generatedObject.GetComponent<Renderer>().bounds.size;
        mainObject.GetComponent<BoxCollider>().size = boundsSize;

        mainObject.transform.rotation = Quaternion.identity;
    }

    private void ApplyHoverEffect(bool isHovering)
    {
        if (isHovering)
        {
            generatedObject.layer = LayerMask.NameToLayer("Hover");
        }
        else
        {
            generatedObject.layer = LayerMask.NameToLayer("Objects");
        }

        this.isHovering = isHovering;
    }

    private void ShowPreviewObject()
    {
        previewObject.GetComponent<MeshRenderer>().enabled = true;
        previewObject.GetComponent<BoxCollider>().enabled = true;
        mainObject.GetComponent<MeshRenderer>().enabled = false;

        Mesh generatedMesh = meshTransformer.GetMesh();

        Mesh newMesh = OffsetVertices(generatedMesh);
        previewObject.GetComponent<MeshFilter>().mesh = Instantiate(newMesh);

        Vector3 boundsSize = generatedObject.GetComponent<Renderer>().bounds.size;
        previewObject.GetComponent<BoxCollider>().size = boundsSize;

        previewObject.transform.rotation = Quaternion.identity;
        previewObject.transform.position = mainObject.transform.position;
    }

    private void HidePreviewObject()
    {
        previewObject.GetComponent<MeshRenderer>().enabled = false;
        previewObject.GetComponent<BoxCollider>().enabled = false;
        mainObject.GetComponent<MeshRenderer>().enabled = true;
    }
}
