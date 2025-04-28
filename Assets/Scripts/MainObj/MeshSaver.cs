using UnityEngine;

public class MeshSaver : MonoBehaviour
{
    public string levelName = "Level_00";

    private MeshFilter meshFilter;
    private BoxCollider boxCollider;
    private UndoManager undoManager;

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

        undoManager = GetComponent<UndoManager>();
        if (undoManager == null)
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

        undoManager.SaveObjectState(currentMesh, localPosition, rotation, colliderSize, -1);

        MeshUtility.SaveLevel(levelName, undoManager.GetUndoStates(), undoManager.GetUndoMeshes());
    }
}
