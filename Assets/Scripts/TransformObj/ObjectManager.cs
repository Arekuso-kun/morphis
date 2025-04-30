using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [Tooltip("The main object to be transformed")]
    public GameObject MainObject;

    [Tooltip("The mode of transformation")]
    public int Mode;

    private ObjectAssigner _objectAssigner;

    void Start()
    {
        if (MainObject == null)
        {
            Debug.LogError("Main object is missing!");
            enabled = false;
            return;
        }

        _objectAssigner = MainObject.GetComponent<ObjectAssigner>();
        if (_objectAssigner == null)
        {
            Debug.LogError("ObjectAssigner component is missing on Main Object!");
            enabled = false;
            return;
        }
    }

    public GameObject GetGrid()
    {
        return _objectAssigner.Grid;
    }

    public GameObject GetObject()
    {
        return _objectAssigner.Object;
    }

    public GameObject GetPreview()
    {
        return _objectAssigner.Preview;
    }
}
