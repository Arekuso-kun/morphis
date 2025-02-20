using UnityEngine;

public class MeshSaver : MonoBehaviour
{
    void Start()
    {

    }

    public void SaveMesh()
    {
        MeshUtility.SaveMeshToFile("test", GetComponent<MeshFilter>());
    }

    public void LoadMesh()
    {
        MeshUtility.LoadMeshFromFile("test", GetComponent<MeshFilter>());
    }
}
