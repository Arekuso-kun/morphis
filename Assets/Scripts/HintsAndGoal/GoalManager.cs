using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(UpdateTrigger))]
public class GoalManager : MonoBehaviour
{
    [Tooltip("The current object being manipulated")]
    [SerializeField] private GameObject _mainObject;

    [Tooltip("The tolerance for comparing vertices")]
    [SerializeField] private float _tolerance = 0.5f;

    [Header("Transition Effects")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CanvasGroup _fadeCanvasGroup;

    public bool IsCorrect { get; private set; } = false;

    private MeshFilter _mainMeshFilter;
    private GridSnap _mainGridSnap;
    private MeshFilter _meshFilter;
    private Renderer _renderer;
    private BoxCollider _boxCollider;
    private CameraController _cameraController;

    private bool _isComparing = false;

    void Awake()
    {
        if (_mainObject == null)
        {
            Debug.LogError("Current object is not assigned!");
            enabled = false;
            return;
        }

        _mainGridSnap = _mainObject.GetComponent<GridSnap>();
        if (_mainGridSnap == null)
        {
            Debug.LogError("GridSnap component not found on the main object!");
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

        if (_mainCamera == null)
        {
            Debug.LogError("Main camera is not assigned!");
            enabled = false;
            return;
        }

        _cameraController = _mainCamera.GetComponent<CameraController>();
        if (_cameraController == null)
        {
            Debug.LogError("CameraController component not found on the main camera!");
            enabled = false;
            return;
        }

        if (_fadeCanvasGroup == null)
        {
            Debug.LogError("Fade CanvasGroup is not assigned!");
            enabled = false;
            return;
        }

        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<Renderer>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    async void Update()
    {
        if (_isComparing) return;

        Vector3[] currentMesh = _mainMeshFilter.mesh.vertices;
        Vector3[] savedMesh = _meshFilter.mesh.vertices;

        if (GetComponent<UpdateTrigger>().NeedsUpdate)
        {
            _isComparing = true;
            bool areEqual = await CompareMeshesAsync(currentMesh, savedMesh);
            _isComparing = false;

            if (areEqual)
            {
                Debug.Log("Meshes are equal, starting animation...");
                // StartCoroutine(AnimateHeight());
                IsCorrect = true;

                StartCoroutine(LoadNextScene());
            }
            else
            {
                Debug.Log("Meshes are not equal, resetting...");
                IsCorrect = false;
            }

            GetComponent<UpdateTrigger>().NeedsUpdate = false;
        }

        if (_mainObject.GetComponent<GridSnap>().IsSnappedToPoint)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<BoxCollider>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    IEnumerator LoadNextScene()
    {
        while (!_mainGridSnap.IsSnappedToPoint)
        {
            if (!IsCorrect) yield break;

            yield return null;
        }

        _cameraController.LockCameraChanges = true;
        _cameraController.LockUserInput = true;
        CameraController.GlobalInteractionLock = true;

        float targetDistance = Mathf.Min(_cameraController.Distance + 5f, _cameraController.MaxDistance);
        float targetFOV = _mainCamera.fieldOfView + 5f;

        Sequence transitionSequence = DOTween.Sequence();

        transitionSequence.Join(DOTween.To(
            () => _cameraController.Distance,
            x => _cameraController.Distance = x,
            targetDistance,
            4f
        ).SetEase(Ease.InOutSine));

        transitionSequence.Join(DOTween.To(
            () => _mainCamera.fieldOfView,
            x => _mainCamera.fieldOfView = x,
            targetFOV,
            4f
        ).SetEase(Ease.InOutSine));

        _fadeCanvasGroup.gameObject.SetActive(true);
        _fadeCanvasGroup.alpha = 0;
        transitionSequence.Insert(
            3f, _fadeCanvasGroup.DOFade(1f, 2f).SetEase(Ease.InOutSine)
        );

        transitionSequence.OnComplete(() =>
        {
            int currentLevel = GetCurrentLevelNumber();
            if (currentLevel > 0)
            {
                PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
                PlayerPrefs.SetInt("HighestLevelUnlocked", Mathf.Max(PlayerPrefs.GetInt("HighestLevelUnlocked", 1), currentLevel + 1));
                PlayerPrefs.Save();
            }
            SceneManager.LoadScene(GetNextSceneName());
        });

        yield return transitionSequence.WaitForCompletion();
    }

    string GetNextSceneName()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene.StartsWith("Level_"))
        {
            string numberPart = currentScene.Substring(6);
            if (int.TryParse(numberPart, out int levelNumber))
            {
                int nextLevelNumber = levelNumber + 1;
                return $"Level_{nextLevelNumber:D2}";
            }
        }

        return "MainMenu";
    }

    int GetCurrentLevelNumber()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.StartsWith("Level_"))
        {
            string numberPart = currentScene.Substring(6);
            if (int.TryParse(numberPart, out int levelNumber))
            {
                return levelNumber;
            }
        }
        return -1;
    }


    async Task<bool> CompareMeshesAsync(Vector3[] meshA, Vector3[] meshB)
    {
        return await Task.Run(() => MeshesAreEqual(meshA, meshB));
    }

    bool MeshesAreEqual(Vector3[] meshA, Vector3[] meshB)
    {
        if (meshA.Length != meshB.Length)
        {
            Debug.Log($"Mesh lengths differ: {meshA.Length} vs {meshB.Length}");
            return false;
        }

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
}
