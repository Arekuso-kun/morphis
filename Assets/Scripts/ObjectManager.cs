using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [Tooltip("The main object to be transformed")]
    public GameObject mainObject;

    [Tooltip("The mode of transformation (1 = circular, 2 = stretch)")]
    public int mode;

    private GameObject targetObject;
    private GameObject targetGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainObject == null)
        {
            Debug.LogError("Main object is missing!");
            return;
        }

        targetObject = mainObject.transform.Find("Object")?.gameObject;
        if (targetObject == null)
        {
            Debug.LogError("Target Object is missing!");
            return;
        }

        targetGrid = mainObject.transform.Find("Grid")?.gameObject;
        if (targetGrid == null)
        {
            Debug.LogError("Target Grid is missing!");
            return;
        }
    }

    public GameObject GetGrid()
    {
        return targetGrid;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
