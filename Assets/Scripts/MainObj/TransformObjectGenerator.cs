using System.Collections.Generic;
using UnityEngine;

public class TransformObjectGenerator : MonoBehaviour
{
    [Tooltip("List of available transformation modes:\n1 = Circular\n2 = Circular Squared\n3 = Stretch\n4 = Shrink\n5 = Wavy\n6 = Wavy Sharp\n7 = Shear")]
    [SerializeField] private List<int> _availableModes = new() { 1, 3, 4, 5 };

    [Tooltip("Prefab for the Transform Object")]
    [SerializeField] private GameObject _transformObjectPrefab;

    [Tooltip("Parent container for generated objects")]
    [SerializeField] private Transform _transformContainer;

    [Tooltip("Custom positions for placing objects (Max 5)")]
    [SerializeField] private List<Vector3> _customPositions = new();

    [Tooltip("Center point offset applied to custom positions")]
    [SerializeField] private Vector3 _containerPosition = Vector3.zero;

    private List<Vector3> _positions = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_transformObjectPrefab == null)
        {
            Debug.LogError("Transform Object Prefab is missing!");
            return;
        }

        if (_transformContainer == null)
        {
            GameObject containerObj = new("Transform Container");
            containerObj.transform.position = _containerPosition;
            _transformContainer = containerObj.transform;
        }

        GeneratePositions();
        GenerateTransformObjects();
    }

    private void GeneratePositions()
    {
        if (_availableModes.Count % 2 == 0)
        {
            _positions.Add(new Vector3(7.5f, 0, 15));
            _positions.Add(new Vector3(-7.5f, 0, 15));
            _positions.Add(new Vector3(17.5f, 0, 0));
            _positions.Add(new Vector3(-17.5f, 0, 0));
        }
        else
        {
            _positions.Add(new Vector3(0, 0, 15));
            _positions.Add(new Vector3(17.5f, 0, 0));
            _positions.Add(new Vector3(-17.5f, 0, 0));
        }

        _positions = _positions.GetRange(0, _availableModes.Count);

        if (_customPositions.Count > 0)
            for (int i = 0; i < _positions.Count; i++)
                _positions[i] = _customPositions[i];
    }

    private void GenerateTransformObjects()
    {
        for (int i = 0; i < _positions.Count; i++)
        {
            Vector3 finalPosition = _containerPosition + _positions[i];
            GameObject newObject = Instantiate(_transformObjectPrefab, finalPosition, Quaternion.identity, _transformContainer);
            newObject.name = "TransformObject_" + _availableModes[i];

            ObjectManager objManager = newObject.GetComponent<ObjectManager>();
            if (objManager == null)
            {
                Debug.LogError("ObjectManager script is missing on the prefab!");
                return;
            }

            objManager.Mode = _availableModes[i];
            objManager.MainObject = this.gameObject;
        }
    }
}
