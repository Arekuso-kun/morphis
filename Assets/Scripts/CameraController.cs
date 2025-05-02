using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Vector3 _target = Vector3.zero;

    [Header("Distance Settings")]
    [SerializeField] private float _distance = 49f;
    [SerializeField] private float _minDistance = 10f;
    [SerializeField] private float _maxDistance = 60f;
    [SerializeField] private float _zoomSpeed = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private Vector2 _rotation = new(0f, 40f);
    [SerializeField] private float _rotationSpeed = 5f;

    [Header("Tilt Settings")]
    [SerializeField] private float _tiltMultiplier = 0.25f;
    [SerializeField] private float _minTilt = 0f;
    [SerializeField] private float _maxTilt = 80f;

    public Vector3 Target
    {
        get => _target;
        set => _target = value;
    }
    public float Distance
    {
        get => _distance;
        set => _distance = value;
    }

    public float Tilt
    {
        get => _rotation.y;
        set => _rotation.y = value;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            _rotation.x += Input.GetAxis("Mouse X") * _rotationSpeed;
            _rotation.y -= Input.GetAxis("Mouse Y") * _rotationSpeed;
            _rotation.y = Mathf.Clamp(_rotation.y, _minTilt, _maxTilt);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _distance -= scroll * _zoomSpeed;
        _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(_rotation.y, _rotation.x, 0f);
        Vector3 direction = new(0f, 0f, -_distance);
        transform.position = _target + rotation * direction;

        Vector3 tiltTarget = new(_target.x, _target.y - _distance * _tiltMultiplier, _target.z);
        transform.LookAt(tiltTarget);
    }
}
