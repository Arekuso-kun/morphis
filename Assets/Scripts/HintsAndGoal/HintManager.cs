using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public GameObject objectsContainer;
    public List<GameObject> hintObjects;

    void Awake()
    {
        if (objectsContainer == null)
        {
            Debug.LogError("Objects container is not assigned!");
            enabled = false;
            return;
        }

        if (hintObjects == null || hintObjects.Count == 0)
        {
            Debug.LogError("Hint objects are not assigned!");
            enabled = false;
            return;
        }
    }

    public void ShowMainObjects()
    {
        objectsContainer.SetActive(true);
        SetHintsActive(false);
    }

    public void ShowHints()
    {
        objectsContainer.SetActive(false);
        SetHintsActive(true);
    }

    private void SetHintsActive(bool active)
    {
        foreach (var obj in hintObjects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}
