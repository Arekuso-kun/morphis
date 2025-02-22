using UnityEngine;
using System.Collections.Generic;

public class UndoManager : MonoBehaviour
{
    private Stack<Mesh> undoMeshStack = new Stack<Mesh>();
    private Stack<Vector3> undoColliderStack = new Stack<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            UndoTransformation();
        }
    }

    public void SaveObjectState(Mesh mesh, Vector3 colliderSize)
    {
        Mesh savedMesh = Instantiate(mesh);

        undoMeshStack.Push(savedMesh);
        undoColliderStack.Push(colliderSize);
    }

    public void UndoTransformation()
    {
        if (undoMeshStack.Count == 0 || undoColliderStack.Count == 0)
        {
            Debug.Log("No previous transformations to undo.");
            return;
        }

        Mesh previousMesh = undoMeshStack.Pop();
        Vector3 previousColliderSize = undoColliderStack.Pop();

        GetComponent<MeshFilter>().mesh = previousMesh;
        GetComponent<BoxCollider>().size = previousColliderSize;
    }
}
