using UnityEngine;
using System.Collections.Generic;

public class TransformOnClick : MonoBehaviour
{
    [Tooltip("The generated object to be assigned to the main object")]
    public GameObject generatedObject;

    private MeshTransformer meshTransformer;
    private Stack<Mesh> undoStack = new Stack<Mesh>();

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
                    SaveMeshState();
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            UndoTransformation();
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

    private void SaveMeshState()
    {
        ObjectManager objectManager = GetComponentInParent<ObjectManager>();
        if (objectManager == null) return;

        GameObject targetObject = objectManager.GetObject();
        if (targetObject == null) return;

        Mesh currentMesh = targetObject.GetComponent<MeshFilter>().mesh;
        if (currentMesh == null) return;

        Mesh savedMesh = Instantiate(currentMesh);
        undoStack.Push(savedMesh);
    }

    // this is broken, fix it
    private void UndoTransformation()
    {
        if (undoStack.Count == 0)
        {
            Debug.Log("No previous transformations to undo.");
            return;
        }

        ObjectManager objectManager = GetComponentInParent<ObjectManager>();
        if (objectManager == null) return;

        GameObject targetObject = objectManager.GetObject();
        if (targetObject == null) return;

        Mesh previousMesh = undoStack.Pop();
        targetObject.GetComponent<MeshFilter>().mesh = previousMesh;
    }
}
