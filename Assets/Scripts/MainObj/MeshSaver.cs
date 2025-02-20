using UnityEngine;
using System.IO;

public class MeshSaver : MonoBehaviour
{
    private string meshSavePath;

    void Start()
    {
        // TO DO
        meshSavePath = Path.Combine(Application.persistentDataPath, "savedMesh.json");
        Debug.Log("Mesh save path: " + meshSavePath);
    }

    public void SaveMesh()
    {
        MeshUtility.SaveMeshToFile(meshSavePath, GetComponent<MeshFilter>());
    }

    public void LoadMesh()
    {
        MeshUtility.LoadMeshFromFile(meshSavePath, GetComponent<MeshFilter>());
    }
}
