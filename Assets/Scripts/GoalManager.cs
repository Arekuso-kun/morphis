using System.IO;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    [Tooltip("The current object being manipulated")]
    public GameObject currentObject;

    private Vector3[] previousMeshVertices;

    void Start()
    {
        // TO DO
        string meshSavePath = Path.Combine(Application.persistentDataPath, "savedMesh.json");
        MeshUtility.LoadMeshFromFile(meshSavePath, GetComponent<MeshFilter>());

        if (currentObject == null)
        {
            Debug.LogError("Current object is missing!");
            return;
        }

        MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component is missing!");
            return;
        }

        previousMeshVertices = meshFilter.mesh.vertices.Clone() as Vector3[];
    }

    void Update()
    {
        if (currentObject == null)
        {
            Debug.LogError("Current object is missing!");
            return;
        }

        MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component is missing!");
            return;
        }

        Vector3[] currentMesh = currentObject.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] savedMesh = GetComponent<MeshFilter>().mesh.vertices;

        if (previousMeshVertices == null || !MeshesAreEqual(previousMeshVertices, currentMesh))
        {
            previousMeshVertices = currentMesh.Clone() as Vector3[];

            if (MeshesAreEqual(currentMesh, savedMesh))
            {
                GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    bool MeshesAreEqual(Vector3[] meshA, Vector3[] meshB)
    {
        if (meshA.Length != meshB.Length)
            return false;

        float tolerance = 0.01f;
        for (int i = 0; i < meshA.Length; i++)
        {
            if (Vector3.Distance(meshA[i], meshB[i]) > tolerance)
                return false;
        }
        return true;
    }
}
