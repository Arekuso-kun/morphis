using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private GameObject _hintObject;
    [SerializeField] private GameObject _goalObject;
    [SerializeField] private GameObject _transformObject;

    private List<ObjectState> _hintStates = new();
    private int _currentHintIndex = -1;
    private string _levelName;

    void Awake()
    {
        _levelName = SceneManager.GetActiveScene().name;

        if (!ValidateObject(_hintObject, "Hint")) return;
        if (!ValidateObject(_goalObject, "Goal")) return;

        if (_transformObject == null)
        {
            Debug.LogError("Transform object is not assigned!");
            enabled = false;
            return;
        }

        if (_transformObject.GetComponent<ObjectManager>() == null)
        {
            Debug.LogError("Transform object does not have an ObjectManager component!");
            enabled = false;
            return;
        }
    }

    private bool ValidateObject(GameObject obj, string name)
    {
        if (obj == null)
        {
            Debug.LogError($"{name} object is not assigned!");
            enabled = false;
            return false;
        }

        if (obj.GetComponent<MeshFilter>() == null)
        {
            Debug.LogError($"{name} object does not have a MeshFilter component!");
            enabled = false;
            return false;
        }

        if (obj.GetComponent<BoxCollider>() == null)
        {
            Debug.LogError($"{name} object does not have a BoxCollider component!");
            enabled = false;
            return false;
        }

        return true;
    }

    void Start()
    {
        Load();

        if (_hintStates.Count > 0)
        {
            _currentHintIndex = 0;
            ApplyState(_hintStates[_currentHintIndex], _hintObject);
        }
    }

    private void Load()
    {
        List<ObjectState> states = MeshUtility.LoadHints(_levelName);

        if (states == null || states.Count == 0)
        {
            Debug.LogError($"No hint data found for level {_levelName}");
            return;
        }

        foreach (var state in states)
        {
            if (state.isGoal)
                ApplyState(state, _goalObject, false);
            else
                _hintStates.Add(state);
        }
    }

    public void ShowNextHint()
    {
        if (_hintStates.Count == 0) return;

        _currentHintIndex++;
        if (_currentHintIndex >= _hintStates.Count)
            _currentHintIndex = _hintStates.Count - 1;

        ApplyState(_hintStates[_currentHintIndex], _hintObject);
    }

    public void ShowPreviousHint()
    {
        if (_hintStates.Count == 0) return;

        _currentHintIndex--;
        if (_currentHintIndex < 0)
            _currentHintIndex = 0;

        ApplyState(_hintStates[_currentHintIndex], _hintObject);
    }

    private void ApplyState(ObjectState state, GameObject target, bool applyMode = true)
    {
        if (state == null || target == null) return;

        Mesh mesh = MeshUtility.LoadMesh(_levelName, state.meshFileName);
        if (mesh == null)
        {
            Debug.LogError($"Mesh not found: {state.meshFileName}");
            return;
        }

        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        target.transform.localPosition = state.position;
        target.transform.localRotation = state.rotation;

        BoxCollider boxCollider = target.GetComponent<BoxCollider>();
        boxCollider.size = state.colliderSize;
        boxCollider.center = Vector3.zero;

        if (applyMode) _transformObject.GetComponent<ObjectManager>().Mode = state.mode;
    }
}
