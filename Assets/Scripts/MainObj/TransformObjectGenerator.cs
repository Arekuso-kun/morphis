using System.Collections.Generic;
using UnityEngine;

public class TransformObjectGenerator : MonoBehaviour
{
    [Tooltip("List of available transformation modes.")]
    [SerializeField]
    private List<TransformationMode> _availableModes = new()
    {
        TransformationMode.Circular,
        TransformationMode.Stretch,
        TransformationMode.Shrink,
        TransformationMode.Wavy
    };


    [Tooltip("Prefab for the Transform Object")]
    [SerializeField] private GameObject _transformObjectPrefab;

    [Tooltip("Parent container for generated objects")]
    [SerializeField] private Transform _transformContainer;

    [Tooltip("Custom positions for placing objects (Max 5)")]
    [SerializeField] private List<Vector3> _customPositions = new();

    [Tooltip("Center point offset applied to custom positions")]
    [SerializeField] private Vector3 _containerPosition = Vector3.zero;

    [Tooltip("List of GameObjects that need to be updated when the object is transformed")]
    [SerializeField] private List<GameObject> _statusUpdate = new();

    private List<Vector3> _positions = new();

    void Start()
    {
        if (_transformObjectPrefab == null)
        {
            Debug.LogError("Transform Object Prefab is missing!");
            enabled = false;
            return;
        }

        foreach (GameObject obj in _statusUpdate)
        {
            if (obj.GetComponent<UpdateTrigger>() == null)
            {
                Debug.LogError("UpdateTrigger script is missing on the object to be updated!");
                enabled = false;
                return;
            }
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
        if (_availableModes.Count == 1)
        {
            _positions.Add(new Vector3(7.5f, 0, -15f));
            transform.position += new Vector3(-7.5f, 0, -2.5f);
            _transformContainer.position += new Vector3(0, 0, 7.5f);
            _containerPosition += new Vector3(0, 0, 7.5f);
        }
        else if (_availableModes.Count % 2 == 0)
        {
            _positions.Add(new Vector3(17.5f, 0, 0));
            _positions.Add(new Vector3(-17.5f, 0, 0));
            _positions.Add(new Vector3(7.5f, 0, 15f));
            _positions.Add(new Vector3(-7.5f, 0, 15f));

            if (_availableModes.Count == 2)
            {
                _transformContainer.position += new Vector3(0, 0, 5f);
                _containerPosition += new Vector3(0, 0, 5f);
            }
        }
        else
        {
            _positions.Add(new Vector3(0, 0, 15f));
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

            foreach (GameObject obj in _statusUpdate)
                newObject.GetComponent<InteractionManager>().StatusUpdate.Add(obj);
        }
    }
}
