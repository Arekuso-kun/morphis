using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridSnap : MonoBehaviour
{
    [Tooltip("The grid object for reference")]
    [SerializeField] private GenerateGrid _grid;
    [SerializeField] private float _moveSmoothSpeed = 20f;

    [Header("Platform Reference")]
    [SerializeField] private BoxCollider platformCollider;
    [SerializeField] private BoxCollider gridAreaCollider;

    [Header("Snap Point")]
    [SerializeField] private GameObject snapPoint;
    [SerializeField] private float snapThreshold = 0.5f;

    [HideInInspector] public bool IsSnappedToPoint = false;
    [HideInInspector] public bool IsDragging = false;
    [HideInInspector] public bool IsRotating = false;
    [HideInInspector] public bool IsOutOfBounds = true;

    private Bounds _platformBounds;
    private Bounds _gridBounds;
    private float _gridSize;
    private Camera _mainCamera;
    private float _halfSize;
    private Vector3 _gridPosition;
    private Vector3 _targetPosition;

    void Awake()
    {
        if (_grid == null)
        {
            Debug.LogError("Grid object not assigned!");
            enabled = false;
            return;
        }

        if (platformCollider == null)
        {
            Debug.LogError("Platform object collider not assigned!");
            enabled = false;
            return;
        }

        if (gridAreaCollider == null)
        {
            Debug.LogError("Grid area collider not assigned!");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        _mainCamera = Camera.main;

        _platformBounds = platformCollider.bounds;
        _gridBounds = gridAreaCollider.bounds;

        _gridSize = _grid.SquareSize / 2.0f;
        _halfSize = _grid.Size * _grid.SquareSize / 2.0f;
        _gridPosition = _grid.transform.position;

        MoveToDefaultPosition();
        _targetPosition = transform.position;
    }

    void Update()
    {
        _platformBounds = platformCollider.bounds;
        _gridBounds = gridAreaCollider.bounds;

        Vector3 currentPosition = transform.position;
        Vector3 checkPosition = new(currentPosition.x, _gridBounds.center.y, currentPosition.z);
        IsOutOfBounds = !_gridBounds.Contains(checkPosition);

        if (CameraController.GlobalInteractionLock) return;

        HandleDragging();
        HandleRotation();

        if (IsDragging)
            transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveSmoothSpeed * Time.deltaTime);
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                IsDragging = true;
                IsSnappedToPoint = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            IsDragging = false;

            if (snapPoint != null)
            {
                Vector3 snapPointPosition = snapPoint.transform.position;
                Vector3 snapPointPositionToCheck = new(snapPointPosition.x, transform.position.y, snapPointPosition.z);
                float distance = Vector3.Distance(transform.position, snapPointPositionToCheck);
                if (distance <= snapThreshold)
                {
                    _targetPosition = new(snapPointPosition.x, transform.position.y, snapPointPosition.z);
                    IsSnappedToPoint = true;
                    transform.position = _targetPosition;
                }
            }
        }

        if (IsDragging)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                Vector3 gridPoint = new(
                    Mathf.Round(point.x / _gridSize) * _gridSize,
                    transform.position.y,
                    Mathf.Round(point.z / _gridSize) * _gridSize
                );

                Vector3 originalPoint = new(point.x, transform.position.y, point.z);
                originalPoint.x = Mathf.Clamp(originalPoint.x, _platformBounds.min.x, _platformBounds.max.x);
                originalPoint.z = Mathf.Clamp(originalPoint.z, _platformBounds.min.z, _platformBounds.max.z);

                IsOutOfBounds = !_gridBounds.Contains(new Vector3(gridPoint.x, _gridBounds.center.y, gridPoint.z));

                _targetPosition = IsOutOfBounds ? originalPoint : gridPoint;
            }
        }
    }

    private void HandleRotation()
    {
        if (IsRotating) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(RotateSmooth(Vector3.right, -90f));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(RotateSmooth(Vector3.right, 90f));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(RotateSmooth(Vector3.up, -90f));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(RotateSmooth(Vector3.up, 90f));
        }
    }

    private IEnumerator RotateSmooth(Vector3 axis, float angle)
    {
        IsRotating = true;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(axis * angle) * startRotation;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        yield return new WaitForSeconds(0.1f);
        IsRotating = false;
    }

    private void MoveToDefaultPosition()
    {
        transform.position = _gridPosition + new Vector3(-(_grid.Size * _grid.SquareSize), transform.position.y, 0);
        _targetPosition = transform.position;
    }
}
