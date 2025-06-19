using UnityEngine;
using System.Collections.Generic;

public class HistoryManager : MonoBehaviour
{
    [SerializeField] private GridSnap _gridSnap;
    [SerializeField] private UpdateTrigger _goalUpdateTrigger;

    private Stack<Mesh> _undoMeshStack = new();
    private Stack<ObjectState> _undoStateStack = new();

    private Stack<Mesh> _redoMeshStack = new();
    private Stack<ObjectState> _redoStateStack = new();

    private Mesh _currentMesh = null;
    private ObjectState _currentState = null;

    private MeshFilter _meshFilter;
    private BoxCollider _boxCollider;

    void Awake()
    {
        if (_gridSnap == null)
        {
            Debug.LogError("GridSnap component is not assigned!");
            enabled = false;
            return;
        }

        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
        {
            Debug.LogError("MeshFilter component is missing!");
            enabled = false;
            return;
        }

        _boxCollider = GetComponent<BoxCollider>();
        if (_boxCollider == null)
        {
            Debug.LogError("BoxCollider component is missing!");
            enabled = false;
            return;
        }

        if (_goalUpdateTrigger == null)
        {
            Debug.LogError("UpdateTrigger component is not assigned!");
            enabled = false;
            return;
        }
    }

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

    public void SaveObjectState(Mesh mesh, Vector3 localPosition, Quaternion rotation, Vector3 colliderSize, TransformationMode mode)
    {
        if (_meshFilter == null || _boxCollider == null) return;

        Mesh lastMesh = Instantiate(mesh);
        ObjectState lastState = new ObjectState
        {
            position = localPosition,
            rotation = rotation,
            colliderSize = colliderSize,
            mode = mode
        };

        _undoMeshStack.Push(lastMesh);
        _undoStateStack.Push(lastState);

        _currentMesh = null;
        _currentState = null;
        _redoMeshStack.Clear();
        _redoMeshStack.Clear();
    }

    public void UndoTransformation()
    {
        if (_undoStateStack.Count == 0)
        {
            Debug.LogWarning("No transformations to undo.");
            return;
        }

        if (_currentMesh != null && _currentState != null)
        {
            _redoMeshStack.Push(Instantiate(_currentMesh));
            _redoStateStack.Push(_currentState);
        }
        else
        {
            _redoMeshStack.Push(Instantiate(_meshFilter.mesh));
            _redoStateStack.Push(new ObjectState
            {
                position = transform.localPosition,
                rotation = transform.rotation,
                colliderSize = _boxCollider.size,
                mode = 0
            });
        }

        _currentMesh = _undoMeshStack.Pop();
        _currentState = _undoStateStack.Pop();
        ApplyChanges();
    }

    public void RedoTransformation()
    {
        if (_redoStateStack.Count == 0)
        {
            Debug.LogWarning("No transformations to redo.");
            return;
        }

        if (_currentMesh != null && _currentState != null)
        {
            _undoMeshStack.Push(Instantiate(_currentMesh));
            _undoStateStack.Push(_currentState);
        }
        else
        {
            _undoMeshStack.Push(Instantiate(_meshFilter.mesh));
            _undoStateStack.Push(new ObjectState
            {
                position = transform.localPosition,
                rotation = transform.rotation,
                colliderSize = _boxCollider.size,
                mode = 0
            });
        }

        _currentMesh = _redoMeshStack.Pop();
        _currentState = _redoStateStack.Pop();
        ApplyChanges();
    }

    private void ApplyChanges()
    {
        _gridSnap.IsSnappedToPoint = false;
        _meshFilter.mesh = _currentMesh;
        _boxCollider.size = _currentState.colliderSize;
        transform.localPosition = _currentState.position;
        transform.rotation = _currentState.rotation;

        _goalUpdateTrigger.NeedsUpdate = true;
    }

    public void ResetTransformations()
    {
        if (_undoStateStack.Count == 0 || _undoMeshStack.Count == 0)
        {
            Debug.LogWarning("No transformations to reset.");
            return;
        }

        Mesh[] meshesArray = _undoMeshStack.ToArray();
        ObjectState[] statesArray = _undoStateStack.ToArray();

        _currentMesh = meshesArray[meshesArray.Length - 1];
        _currentState = statesArray[statesArray.Length - 1];
        ApplyChanges();

        _undoMeshStack.Clear();
        _undoStateStack.Clear();
        _redoMeshStack.Clear();
        _redoStateStack.Clear();

        _currentMesh = null;
        _currentState = null;

        transform.localPosition = new Vector3(0, 1, 0);
        transform.rotation = Quaternion.identity;
    }

    public Stack<ObjectState> GetUndoStates()
    {
        return new Stack<ObjectState>(_undoStateStack);
    }

    public Stack<Mesh> GetUndoMeshes()
    {
        return new Stack<Mesh>(_undoMeshStack);
    }
}
