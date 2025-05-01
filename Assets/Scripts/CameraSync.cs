using UnityEngine;

public class CameraSync : MonoBehaviour
{
    [Tooltip("The camera to sync with")]
    [SerializeField] private Camera _targetCamera;

    void Start()
    {
        if (_targetCamera == null)
        {
            Debug.LogWarning("Target camera not assigned.");
            return;
        }

        transform.position = _targetCamera.transform.position;
        transform.rotation = _targetCamera.transform.rotation;
    }
}
