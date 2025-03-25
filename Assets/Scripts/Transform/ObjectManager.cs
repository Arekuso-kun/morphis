using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [Tooltip("The main object to be transformed")]
    public GameObject mainObject;

    [Tooltip("The mode of transformation")]
    public int mode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainObject == null)
        {
            Debug.LogError("Main object is missing!");
            return;
        }

        Debug.Log("ObjectManager initialized successfully.");
    }

    public GameObject GetGrid()
    {
        return mainObject.transform.Find("Grid")?.gameObject;
    }

    public GameObject GetObject()
    {
        return mainObject.transform.Find("Object")?.gameObject;
    }

    public GameObject GetPreview()
    {
        return mainObject.transform.Find("Preview")?.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
