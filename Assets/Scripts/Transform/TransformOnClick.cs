using UnityEngine;

public class TransformOnClick : MonoBehaviour
{
    [Tooltip("The generated object to be assigned to the main object")]
    public GameObject generatedObject;

    private MeshTransformer meshTransformer;

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
    }

    void Update()
    {
        if (generatedObject == null || meshTransformer == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform == generatedObject.transform)
            {
                ApplyHoverEffect(true);
                if (Input.GetMouseButtonDown(0)) // Left click
                {
                    SaveObjectState();
                    ApplyTransformation();
                }
            }
            else
            {
                ApplyHoverEffect(false);
            }
        }
        else
        {
            ApplyHoverEffect(false);
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

    private void ApplyTransformation()
    {
        Mesh generatedMesh = meshTransformer.GetMesh();
        ObjectManager objectManager = GetComponentInParent<ObjectManager>();
        if (objectManager == null)
        {
            Debug.LogError("ObjectManager is missing!");
            return;
        }

        GameObject targetObject = objectManager.GetObject();
        if (targetObject == null)
        {
            Debug.LogError("Target Object is missing!");
            return;
        }

        Vector3[] vertices = generatedMesh.vertices;
        Vector3 center = generatedObject.GetComponent<Renderer>().bounds.center - transform.position;
        center.y = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= center;
        }

        Mesh newMesh = Instantiate(generatedMesh);
        newMesh.vertices = vertices;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        targetObject.GetComponent<MeshFilter>().mesh = newMesh;

        Vector3 boundsSize = generatedObject.GetComponent<Renderer>().bounds.size;
        targetObject.GetComponent<BoxCollider>().size = boundsSize;

        targetObject.transform.rotation = Quaternion.identity;
    }

    private void ApplyHoverEffect(bool isHovering)
    {
        if (isHovering)
        {
            generatedObject.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        }
        else
        {
            generatedObject.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        }
    }
}
