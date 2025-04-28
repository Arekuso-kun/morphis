using UnityEngine;

public class MeshSaver : MonoBehaviour
{
    public string levelName = "Level_00";

    private MeshFilter meshFilter;
    private BoxCollider boxCollider;
    private HistoryManager historyManager;

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

        historyManager = GetComponent<HistoryManager>();
        if (historyManager == null)
        {
            Debug.LogError("UndoManager component is missing!");
            enabled = false;
            return;
        }
    }

    public void SaveMesh()
    {
        Mesh currentMesh = meshFilter.mesh;
        Vector3 colliderSize = boxCollider.size;
        Vector3 localPosition = transform.localPosition;
        Quaternion rotation = transform.rotation;

        historyManager.SaveObjectState(currentMesh, localPosition, rotation, colliderSize, -1);

        MeshUtility.SaveLevel(levelName, historyManager.GetUndoStates(), historyManager.GetUndoMeshes());
    }
}
