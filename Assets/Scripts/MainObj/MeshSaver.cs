using UnityEngine;
using System.IO;

public class MeshSaver : MonoBehaviour
{
    private string meshSavePath;

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
