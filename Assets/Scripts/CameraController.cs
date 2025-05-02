using UnityEngine;
using DG.Tweening;

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

    [Header("Reset UI")]
    [SerializeField] private SlideAnimation _resetButtonPanel;

    [HideInInspector] public bool LockCameraChanges = false;
    [HideInInspector] public bool LockUserInput = false;

    private float _originalDistance;
    private Vector2 _originalRotation;
    private Vector3 _originalTarget;
    private bool _buttonShown = false;
    private bool _isResettingCamera = false;

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

    void Start()
    {
        _originalDistance = _distance;
        _originalRotation = _rotation;
        _originalTarget = _target;
    }

    void Update()
    {
        if (LockUserInput || _isResettingCamera) return;

        if (Input.GetMouseButton(1))
        {
            _rotation.x += Input.GetAxis("Mouse X") * _rotationSpeed;
            _rotation.y -= Input.GetAxis("Mouse Y") * _rotationSpeed;
            _rotation.y = Mathf.Clamp(_rotation.y, _minTilt, _maxTilt);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _distance -= scroll * _zoomSpeed;
        _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);

        if (HasChanged && !_buttonShown)
        {
            _resetButtonPanel.gameObject.SetActive(true);
            _buttonShown = true;
        }
        else if (!HasChanged && _buttonShown)
        {
            _resetButtonPanel.ClosePanel();
            _buttonShown = false;
        }
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(_rotation.y, _rotation.x, 0f);
        Vector3 direction = new(0f, 0f, -_distance);
        transform.position = _target + rotation * direction;

        Vector3 tiltTarget = new(_target.x, _target.y - _distance * _tiltMultiplier, _target.z);
        transform.LookAt(tiltTarget);
    }

    public bool HasChanged
    {
        get
        {
            if (LockCameraChanges || _isResettingCamera) return false;

            return
                !Mathf.Approximately(_distance, _originalDistance) ||
                !Mathf.Approximately(_rotation.x, _originalRotation.x) ||
                !Mathf.Approximately(_rotation.y, _originalRotation.y) ||
                _target != _originalTarget;
        }
    }


    public void ResetCamera()
    {
        _isResettingCamera = true;

        Sequence resetSequence = DOTween.Sequence();

        resetSequence.Join(DOTween.To(() => _distance, x => _distance = x, _originalDistance, 0.5f).SetEase(Ease.OutExpo));
        resetSequence.Join(DOTween.To(() => _rotation.x, x => _rotation.x = x, _originalRotation.x, 0.5f).SetEase(Ease.OutExpo));
        resetSequence.Join(DOTween.To(() => _rotation.y, x => _rotation.y = x, _originalRotation.y, 0.5f).SetEase(Ease.OutExpo));
        resetSequence.Join(DOTween.To(() => _target, x => _target = x, _originalTarget, 0.5f).SetEase(Ease.OutExpo));

        resetSequence.OnComplete(() =>
        {
            _isResettingCamera = false;
        });
    }
}
