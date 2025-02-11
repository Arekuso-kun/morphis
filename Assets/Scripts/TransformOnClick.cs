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

        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                ApplyTransformation();
            }
        }
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
        targetObject.GetComponent<MeshFilter>().mesh = newMesh;

        Vector3 boundsSize = generatedObject.GetComponent<Renderer>().bounds.size;
        targetObject.GetComponent<BoxCollider>().size = boundsSize;
    }
}
