using UnityEngine;

public class ObjectAssigner : MonoBehaviour
{
    public GameObject Grid;

    public GameObject Object;

    public GameObject Preview;

    void Awake()
    {
        if (Grid == null)
        {
            Debug.LogError("Grid object is not assigned!");
            enabled = false;
            return;
        }

        if (Object == null)
        {
            Debug.LogError("Main sub-object is not assigned!");
            enabled = false;
            return;
        }

        if (Preview == null)
        {
            Debug.LogError("Preview object is not assigned!");
            enabled = false;
            return;
        }
    }
}
