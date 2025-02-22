using UnityEngine;

public class MeshSaver : MonoBehaviour
{
    void Start()
    {

    }

    public void SaveMesh()
    {
        MeshUtility.SaveMeshToFile("savedmesh", GetComponent<MeshFilter>());
    }
}
