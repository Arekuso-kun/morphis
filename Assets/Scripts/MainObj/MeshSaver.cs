using UnityEngine;
using UnityEngine.SceneManagement;

public class MeshSaver : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private BoxCollider _boxCollider;
    private HistoryManager _historyManager;
    private string _levelName;

    void Awake()
    {
        _levelName = SceneManager.GetActiveScene().name;

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

        _historyManager = GetComponent<HistoryManager>();
        if (_historyManager == null)
        {
            Debug.LogError("UndoManager component is missing!");
            enabled = false;
            return;
        }
    }

    public void SaveMesh()
    {
        Mesh currentMesh = _meshFilter.mesh;
        Vector3 colliderSize = _boxCollider.size;
        Vector3 localPosition = transform.localPosition;
        Quaternion rotation = transform.rotation;

        _historyManager.SaveObjectState(currentMesh, localPosition, rotation, colliderSize, 0);

        MeshUtility.SaveLevel(_levelName, _historyManager.GetUndoStates(), _historyManager.GetUndoMeshes());
    }
}
