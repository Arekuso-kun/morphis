using System.Collections.Generic;
using UnityEngine;

public class TransformObjectGenerator : MonoBehaviour
{
    [Tooltip("List of available transformation modes:\n1 = Circular\n2 = Stretch\n3 = Shrink\n4 = Wavy")]
    public List<int> availableModes = new List<int> { 1, 2, 3, 4 };

    [Tooltip("Prefab for the Transform Object")]
    public GameObject transformObjectPrefab;

    [Tooltip("Parent container for generated objects")]
    public Transform transformContainer;

    [Tooltip("Center point for the grid layout")]
    public Vector3 containerPosition = Vector3.zero;

    [Tooltip("Maximum columns in the grid layout")]
    public int maxColumns = 2;

    [Tooltip("Spacing between objects")]
    public float spacing = 20.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transformObjectPrefab == null)
        {
            Debug.LogError("Transform Object Prefab is missing!");
            return;
        }

        if (transformContainer == null)
        {
            GameObject containerObj = new("Transform Container");
            containerObj.transform.position = containerPosition;
            transformContainer = containerObj.transform;
        }

        GenerateTransformObjects();
    }

    // private void GenerateTransformObjects()
    // {
    //     foreach (int mode in availableModes)
    //     {
    //         GameObject newObject = Instantiate(transformObjectPrefab, transformContainer);
    //         ObjectManager objManager = newObject.GetComponent<ObjectManager>();

    //         if (objManager != null)
    //         {
    //             objManager.mode = mode;
    //             objManager.mainObject = this.gameObject;

    //             Debug.Log($"Generated Transform Object with mode: {mode}");
    //         }
    //         else
    //         {
    //             Debug.LogError("ObjectManager script is missing on the prefab!");
    //         }
    //     }
    // }

    // Update is called once per frame
    private void GenerateTransformObjects()
    {
        int totalObjects = availableModes.Count;
        int columns = Mathf.Min(maxColumns, totalObjects);
        int rows = Mathf.CeilToInt((float)totalObjects / columns);

        float totalWidth = (columns - 1) * spacing;
        float totalHeight = (rows - 1) * spacing;
        Vector3 startPosition = new Vector3(-totalWidth / 2, 0, totalHeight / 2);

        for (int i = 0; i < totalObjects; i++)
        {
            float positionX = i % columns * spacing;
            float positionZ = i / columns * spacing;

            Vector3 position = startPosition + new Vector3(positionX, 0, -positionZ);

            GameObject newObject = Instantiate(transformObjectPrefab, containerPosition + position, Quaternion.identity, transformContainer);

            ObjectManager objManager = newObject.GetComponent<ObjectManager>();
            if (objManager == null)
            {
                Debug.LogError("ObjectManager script is missing on the prefab!");
                return;
            }

            objManager.mode = availableModes[i];
            objManager.mainObject = this.gameObject;
            Debug.Log($"Generated Transform Object with mode: {availableModes[i]} at position {position}");
        }
    }

    void Update()
    {

    }
}
