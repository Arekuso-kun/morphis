using UnityEngine;
using System.Collections.Generic;

public class HistoryManager : MonoBehaviour
{
    private Stack<Mesh> undoMeshStack = new Stack<Mesh>();
    private Stack<ObjectState> undoStateStack = new Stack<ObjectState>();

    private Stack<Mesh> redoMeshStack = new Stack<Mesh>();
    private Stack<ObjectState> redoStateStack = new Stack<ObjectState>();

    private Mesh currentMesh = null;
    private ObjectState currentState = null;

    private MeshFilter meshFilter;
    private BoxCollider boxCollider;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component is missing!");
            enabled = false;
            return;
        }

        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider component is missing!");
            enabled = false;
            return;
        }
    }

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

        if (Input.GetKeyDown(KeyCode.Y))
        {
            RedoTransformation();
        }
    }

    public void SaveObjectState(Mesh mesh, Vector3 localPosition, Quaternion rotation, Vector3 colliderSize, int mode)
    {
        if (meshFilter == null || boxCollider == null) return;

        Mesh lastMesh = Instantiate(mesh);
        ObjectState lastState = new ObjectState
        {
            position = localPosition,
            rotation = rotation,
            colliderSize = colliderSize,
            mode = mode
        };

        undoMeshStack.Push(lastMesh);
        undoStateStack.Push(lastState);

        currentMesh = null;
        currentState = null;
        redoMeshStack.Clear();
        redoMeshStack.Clear();
    }

    public void UndoTransformation()
    {
        if (undoStateStack.Count == 0)
        {
            Debug.LogWarning("No transformations to undo.");
            return;
        }

        if (currentMesh != null && currentState != null)
        {
            redoMeshStack.Push(Instantiate(currentMesh));
            redoStateStack.Push(currentState);
        }
        else
        {
            redoMeshStack.Push(Instantiate(meshFilter.mesh));
            redoStateStack.Push(new ObjectState
            {
                position = transform.localPosition,
                rotation = transform.rotation,
                colliderSize = boxCollider.size,
                mode = -1
            });
        }

        currentMesh = undoMeshStack.Pop();
        currentState = undoStateStack.Pop();
        ApplyChanges();
    }

    public void RedoTransformation()
    {
        if (redoStateStack.Count == 0)
        {
            Debug.LogWarning("No transformations to redo.");
            return;
        }

        if (currentMesh != null && currentState != null)
        {
            undoMeshStack.Push(Instantiate(currentMesh));
            undoStateStack.Push(currentState);
        }
        else
        {
            undoMeshStack.Push(Instantiate(meshFilter.mesh));
            undoStateStack.Push(new ObjectState
            {
                position = transform.localPosition,
                rotation = transform.rotation,
                colliderSize = boxCollider.size,
                mode = -1
            });
        }

        currentMesh = redoMeshStack.Pop();
        currentState = redoStateStack.Pop();
        ApplyChanges();
    }

    private void ApplyChanges()
    {
        meshFilter.mesh = currentMesh;
        boxCollider.size = currentState.colliderSize;
        transform.localPosition = currentState.position;
        transform.rotation = currentState.rotation;
    }

    public void ResetTransformations()
    {
        if (undoStateStack.Count == 0 || undoMeshStack.Count == 0)
        {
            Debug.LogWarning("No transformations to reset.");
            return;
        }

        Mesh[] meshesArray = undoMeshStack.ToArray();
        ObjectState[] statesArray = undoStateStack.ToArray();

        currentMesh = meshesArray[meshesArray.Length - 1];
        currentState = statesArray[statesArray.Length - 1];
        ApplyChanges();

        undoMeshStack.Clear();
        undoStateStack.Clear();
        redoMeshStack.Clear();
        redoStateStack.Clear();

        currentMesh = null;
        currentState = null;

        transform.localPosition = new Vector3(0, 1, 0);
        transform.rotation = Quaternion.identity;
    }

    public Stack<ObjectState> GetUndoStates()
    {
        return new Stack<ObjectState>(undoStateStack);
    }

    public Stack<Mesh> GetUndoMeshes()
    {
        return new Stack<Mesh>(undoMeshStack);
    }
}
