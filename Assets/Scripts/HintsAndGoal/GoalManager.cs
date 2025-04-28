using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class GoalManager : MonoBehaviour
{
    [Tooltip("The current object being manipulated")]
    public GameObject currentObject;

    [Tooltip("The tolerance for comparing vertices")]
    public float tolerance = 0.5f;

    private Vector3[] previousMeshVertices;
    private bool isComparing = false;
    private Material material;

    [Header("Height Animation Settings")]
    public float heightDuration = 2f;
    private float startHeight = -1f;

    [Header("Emission Pulse Settings")]
    public float pulseDuration = 0.3f;
    public float peakEmission = 2f;
    public float baseEmission = 0f;

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

        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Renderer component is missing!");
            return;
        }

        material = rend.material;
        material.SetFloat("_Height", startHeight);
        material.SetFloat("_Emission", baseEmission);

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

            if (areEqual)
            {
                Debug.Log("Meshes are equal, starting animation...");
                StartCoroutine(AnimateHeight());
            }
        }
    }

    IEnumerator AnimateHeight()
    {
        float currentHeight;
        float elapsed = 0f;

        Vector3 boundsSize = GetComponent<Renderer>().bounds.size;
        float targetHeight = boundsSize.y + 1f;

        while (elapsed < heightDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / heightDuration);
            currentHeight = Mathf.Lerp(startHeight, targetHeight, t);
            material.SetFloat("_Height", currentHeight);
            yield return null;
        }

        currentHeight = targetHeight;
        material.SetFloat("_Height", currentHeight);

        StartCoroutine(EmissionPulse());
    }

    IEnumerator EmissionPulse()
    {
        float startEmission = baseEmission;
        float targetEmission = peakEmission;
        float halfDuration = pulseDuration / 2f;

        // up
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float emission = Mathf.Lerp(startEmission, targetEmission, t);
            material.SetFloat("_Emission", emission);
            yield return null;
        }

        // down
        startEmission = peakEmission;
        targetEmission = baseEmission;
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float emission = Mathf.Lerp(startEmission, targetEmission, t);
            material.SetFloat("_Emission", emission);
            yield return null;
        }

        material.SetFloat("_Emission", baseEmission);
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

        var sortedA = meshA.OrderBy(v => v.x).ThenBy(v => v.y).ThenBy(v => v.z).ToArray();
        var sortedB = meshB.OrderBy(v => v.x).ThenBy(v => v.y).ThenBy(v => v.z).ToArray();

        int[] steps = { 1000, 100, 10 };

        foreach (int step in steps)
        {
            if (!MatchVerticesEveryN(sortedA, sortedB, step, $"Quick check (step {step})"))
                return false;
        }

        if (!MatchRemainingVertices(sortedA, sortedB, steps, "Full check"))
            return false;

        return true;
    }

    bool MatchVerticesEveryN(Vector3[] sortedA, Vector3[] sortedB, int step, string debugLabel)
    {
        for (int i = 0; i < sortedA.Length; i += step)
        {
            if (!MatchSingleVertex(sortedA[i], i, sortedB, debugLabel))
                return false;
        }
        return true;
    }

    bool MatchRemainingVertices(Vector3[] sortedA, Vector3[] sortedB, int[] steps, string debugLabel)
    {
        for (int i = 0; i < sortedA.Length; i++)
        {
            bool alreadyChecked = steps.Any(step => i % step == 0);
            if (alreadyChecked) continue;

            if (!MatchSingleVertex(sortedA[i], i, sortedB, debugLabel))
                return false;
        }
        return true;
    }

    bool MatchSingleVertex(Vector3 pointA, int index, Vector3[] sortedB, string debugLabel)
    {
        bool foundMatch = false;
        int offset = 0;

        while (true)
        {
            int forwardIndex = index + offset;
            int backwardIndex = index - offset;

            if (forwardIndex >= sortedB.Length && backwardIndex < 0)
                break;

            if (forwardIndex < sortedB.Length)
            {
                Vector3 pointB = sortedB[forwardIndex];
                if (WithinTolerance(pointA, pointB))
                {
                    foundMatch = true;
                    break;
                }
            }

            if (backwardIndex >= 0)
            {
                Vector3 pointB = sortedB[backwardIndex];
                if (WithinTolerance(pointA, pointB))
                {
                    foundMatch = true;
                    break;
                }
            }

            offset++;
        }

        if (!foundMatch)
        {
            Debug.Log($"{debugLabel}: No match found for vertex {index}: {pointA}, offset: {offset}, forwardIndex: {index + offset}, backwardIndex: {index - offset}");
        }

        return foundMatch;
    }

    bool WithinTolerance(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) <= tolerance
            && Mathf.Abs(a.y - b.y) <= tolerance
            && Mathf.Abs(a.z - b.z) <= tolerance;
    }

    void UpdateCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        Vector3 boundsSize = GetComponent<Renderer>().bounds.size;
        boundsSize.y = Mathf.Max(boundsSize.y, 0.01f);
        boxCollider.size = boundsSize;
    }
}
