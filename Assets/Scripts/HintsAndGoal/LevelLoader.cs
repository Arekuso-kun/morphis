using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public string levelName = "Level_00";
    public GameObject hintObject;
    public GameObject goalObject;

    private List<ObjectState> hintStates = new();
    private int currentHintIndex = -1;

    void Awake()
    {
        if (!ValidateObject(hintObject, "Hint")) return;
        if (!ValidateObject(goalObject, "Goal")) return;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Load();

        if (hintStates.Count > 0)
        {
            currentHintIndex = 0;
            ApplyState(hintStates[currentHintIndex], hintObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Load()
    {
        List<ObjectState> states = MeshUtility.LoadHints(levelName);

        if (states == null || states.Count == 0)
        {
            Debug.LogError($"No hint data found for level {levelName}");
            return;
        }

        foreach (var state in states)
        {
            if (state.isGoal)
                ApplyState(state, goalObject);
            else
                hintStates.Add(state);
        }
    }

    public void ShowNextHint()
    {
        if (hintStates.Count == 0) return;

        currentHintIndex++;
        if (currentHintIndex >= hintStates.Count)
            currentHintIndex = hintStates.Count - 1;

        ApplyState(hintStates[currentHintIndex], hintObject);
    }

    public void ShowPreviousHint()
    {
        if (hintStates.Count == 0) return;

        currentHintIndex--;
        if (currentHintIndex < 0)
            currentHintIndex = 0;

        ApplyState(hintStates[currentHintIndex], hintObject);
    }

    private void ApplyState(ObjectState state, GameObject target)
    {
        if (state == null || target == null) return;

        Mesh mesh = MeshUtility.LoadMesh(levelName, state.meshFileName);
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

    }
}
