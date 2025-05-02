using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [Tooltip("The main object to be transformed")]
    public GameObject MainObject;

    [Tooltip("The mode of transformation")]
    public TransformationMode Mode;

    void Start()
    {
        if (MainObject == null)
        {
            Debug.LogError("Main object is missing!");
            enabled = false;
            return;
        }
    }

    public GameObject GetGrid()
    {
        return MainObject.transform.Find("Grid")?.gameObject;
    }

    public GameObject GetObject()
    {
        return MainObject.transform.Find("Object")?.gameObject;
    }

    public GameObject GetPreview()
    {
        return MainObject.transform.Find("Preview")?.gameObject;
    }
}
