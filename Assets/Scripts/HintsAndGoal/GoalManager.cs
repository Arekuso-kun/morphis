using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class GoalManager : MonoBehaviour
{
    [Tooltip("The current object being manipulated")]
    [SerializeField] private GameObject _mainObject;

    [Tooltip("The tolerance for comparing vertices")]
    [SerializeField] private float _tolerance = 0.5f;

    [Header("Height Animation Settings")]
    [SerializeField] private float _heightDuration = 2f;
    [SerializeField] private float _startHeight = -1f;

    [Header("Emission Pulse Settings")]
    [SerializeField] private float _pulseDuration = 0.3f;
    [SerializeField] private float _peakEmission = 2f;
    [SerializeField] private float _baseEmission = 0f;

    private MeshFilter _mainMeshFilter;
    private MeshFilter _meshFilter;
    private Renderer _renderer;
    private BoxCollider _boxCollider;

    private Vector3[] _previousMeshVertices;
    private bool _isComparing = false;

    void Awake()
    {
        if (_mainObject == null)
        {
            Debug.LogError("Current object is not assigned!");
            enabled = false;
            return;
        }

        _mainMeshFilter = _mainObject.GetComponent<MeshFilter>();
        if (_mainMeshFilter == null)
        {
            Debug.LogError("MeshFilter component not found on the main object!");
            enabled = false;
            return;
        }
        _previousMeshVertices = (Vector3[])_mainMeshFilter.mesh.vertices.Clone();

        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<Renderer>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    void Start()
    {

        _renderer.material.SetFloat("_Height", _startHeight);
        _renderer.material.SetFloat("_Emission", _baseEmission);

        UpdateCollider();
    }

    async void Update()
    {
        if (_mainObject == null || _isComparing) return;

        Vector3[] currentMesh = _mainMeshFilter.mesh.vertices;
        Vector3[] savedMesh = _meshFilter.mesh.vertices;

        if (_previousMeshVertices == null || MeshChanged(currentMesh))
        {
            _previousMeshVertices = (Vector3[])currentMesh.Clone();

            _isComparing = true;
            bool areEqual = await CompareMeshesAsync(currentMesh, savedMesh);
            _isComparing = false;

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

        Vector3 boundsSize = _renderer.bounds.size;
        float targetHeight = boundsSize.y + 1f;

        while (elapsed < _heightDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _heightDuration);
            currentHeight = Mathf.Lerp(_startHeight, targetHeight, t);
            _renderer.material.SetFloat("_Height", currentHeight);
            yield return null;
        }

        currentHeight = targetHeight;
        _renderer.material.SetFloat("_Height", currentHeight);

        StartCoroutine(EmissionPulse());
    }

    IEnumerator EmissionPulse()
    {
        float startEmission = _baseEmission;
        float targetEmission = _peakEmission;
        float halfDuration = _pulseDuration / 2f;

        // up
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float emission = Mathf.Lerp(startEmission, targetEmission, t);
            _renderer.material.SetFloat("_Emission", emission);
            yield return null;
        }

        // down
        startEmission = _peakEmission;
        targetEmission = _baseEmission;
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float emission = Mathf.Lerp(startEmission, targetEmission, t);
            _renderer.material.SetFloat("_Emission", emission);
            yield return null;
        }

        _renderer.material.SetFloat("_Emission", _baseEmission);
    }

    bool MeshChanged(Vector3[] currentMesh)
    {
        return !currentMesh.SequenceEqual(_previousMeshVertices);
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
        return Mathf.Abs(a.x - b.x) <= _tolerance
            && Mathf.Abs(a.y - b.y) <= _tolerance
            && Mathf.Abs(a.z - b.z) <= _tolerance;
    }

    void UpdateCollider()
    {
        Vector3 boundsSize = _renderer.bounds.size;
        boundsSize.y = Mathf.Max(boundsSize.y, 0.01f);
        _boxCollider.size = boundsSize;
    }
}
