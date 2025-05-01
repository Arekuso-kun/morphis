using UnityEngine;

public class CameraSync : MonoBehaviour
{
    [Tooltip("The camera to sync with")]
    [SerializeField] private Camera _targetCamera;

    void Awake()
    {
        if (_targetCamera == null)
        {
            Debug.LogWarning("Target camera not assigned.");
            return;
        }
    }

    void Update()
    {
        transform.SetPositionAndRotation(_targetCamera.transform.position, _targetCamera.transform.rotation);
    }
}
