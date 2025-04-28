using System.Collections.Generic;
using UnityEngine;

public class TransformObjectGenerator : MonoBehaviour
{
    [Tooltip("List of available transformation modes:\n1 = Circular\n2 = Circular Squared\n3 = Stretch\n4 = Shrink\n5 = Wavy\n6 = Wavy Sharp\n7 = Shear")]
    public List<int> availableModes = new List<int> { 1, 3, 4, 5 };

    [Tooltip("Prefab for the Transform Object")]
    public GameObject transformObjectPrefab;

    [Tooltip("Parent container for generated objects")]
    public Transform transformContainer;

    [Tooltip("Custom positions for placing objects (Max 5)")]
    public List<Vector3> customPositions = new List<Vector3>();

    [Tooltip("Center point offset applied to custom positions")]
    public Vector3 containerPosition = Vector3.zero;

    private List<Vector3> positions = new List<Vector3>();

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

        GeneratePositions();
        GenerateTransformObjects();
    }

    private void GeneratePositions()
    {
        if (availableModes.Count % 2 == 0)
        {
            positions.Add(new Vector3(7.5f, 0, 15));
            positions.Add(new Vector3(-7.5f, 0, 15));
            positions.Add(new Vector3(17.5f, 0, 0));
            positions.Add(new Vector3(-17.5f, 0, 0));
        }
        else
        {
            positions.Add(new Vector3(0, 0, 15));
            positions.Add(new Vector3(17.5f, 0, 0));
            positions.Add(new Vector3(-17.5f, 0, 0));
        }

        positions = positions.GetRange(0, availableModes.Count);

        if (customPositions.Count > 0)
            for (int i = 0; i < positions.Count; i++)
                positions[i] = customPositions[i];
    }

    private void GenerateTransformObjects()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 finalPosition = containerPosition + positions[i];
            GameObject newObject = Instantiate(transformObjectPrefab, finalPosition, Quaternion.identity, transformContainer);
            newObject.name = "TransformObject_" + availableModes[i];

            ObjectManager objManager = newObject.GetComponent<ObjectManager>();
            if (objManager == null)
            {
                Debug.LogError("ObjectManager script is missing on the prefab!");
                return;
            }

            objManager.mode = availableModes[i];
            objManager.mainObject = this.gameObject;
        }
    }

    void Update()
    {

    }
}
