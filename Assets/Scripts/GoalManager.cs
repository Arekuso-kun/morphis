using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class GoalManager : MonoBehaviour
{
    [Tooltip("The current object being manipulated")]
    public GameObject currentObject;

    private Vector3[] previousMeshVertices;
    private bool isComparing = false;

    void Start()
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

        previousMeshVertices = (Vector3[])meshFilter.mesh.vertices.Clone();

        UpdateCollider();
    }

    async void Update()
    {
        if (currentObject == null || isComparing) return;

        MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Vector3[] currentMesh = meshFilter.mesh.vertices;
        Vector3[] savedMesh = GetComponent<MeshFilter>().mesh.vertices;

        if (previousMeshVertices == null || MeshChanged(currentMesh))
        {
            previousMeshVertices = (Vector3[])currentMesh.Clone();

            isComparing = true;
            bool areEqual = await CompareMeshesAsync(currentMesh, savedMesh);
            isComparing = false;

            GetComponent<Renderer>().material.color = areEqual ? Color.green : Color.red;
        }
    }

    bool MeshChanged(Vector3[] currentMesh)
    {
        return !currentMesh.SequenceEqual(previousMeshVertices);
    }

    async Task<bool> CompareMeshesAsync(Vector3[] meshA, Vector3[] meshB)
    {
        return await Task.Run(() => MeshesAreEqual(meshA, meshB));
    }

    bool MeshesAreEqual(Vector3[] meshA, Vector3[] meshB)
    {
        if (meshA.Length != meshB.Length) return false;

        float tolerance = 0.75f;

        var sortedA = meshA.OrderBy(v => v.x).ThenBy(v => v.y).ThenBy(v => v.z).ToArray();
        var sortedB = meshB.OrderBy(v => v.x).ThenBy(v => v.y).ThenBy(v => v.z).ToArray();

        for (int i = 0; i < sortedA.Length; i++)
        {
            bool foundMatch = false;
            Vector3 pointA = sortedA[i];

            for (int offset = 0; ; offset++)
            {
                int forwardIndex = i + offset;
                int backwardIndex = i - offset;

                if (forwardIndex >= sortedB.Length && backwardIndex < 0)
                    break;

                if (forwardIndex < sortedB.Length)
                {
                    Vector3 pointB = sortedB[forwardIndex];
                    if (Mathf.Abs(pointA.x - pointB.x) <= tolerance
                        && Mathf.Abs(pointA.y - pointB.y) <= tolerance
                        && Mathf.Abs(pointA.z - pointB.z) <= tolerance)
                    {
                        foundMatch = true;
                        break;
                    }
                }

                if (backwardIndex >= 0)
                {
                    Vector3 pointB = sortedB[backwardIndex];
                    if (Mathf.Abs(pointA.x - pointB.x) <= tolerance
                        && Mathf.Abs(pointA.y - pointB.y) <= tolerance
                        && Mathf.Abs(pointA.z - pointB.z) <= tolerance)
                    {
                        foundMatch = true;
                        break;
                    }
                }
            }

            if (!foundMatch) return false;
        }

        return true;
    }

    void UpdateCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        Vector3 boundsSize = GetComponent<Renderer>().bounds.size;
        boundsSize.y = Mathf.Max(boundsSize.y, 0.01f);
        boxCollider.size = boundsSize;
    }
}
