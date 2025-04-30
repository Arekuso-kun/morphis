using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridSnap : MonoBehaviour
{
    [Tooltip("The grid object for reference")]
    [SerializeField] private GenerateGrid _grid;
    [SerializeField] private float _moveSmoothSpeed = 20f;

    public bool IsDragging = false;
    public bool IsRotating = false;

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
    }

    void Start()
    {
        _mainCamera = Camera.main;
        _gridSize = _grid.SquareSize / 2.0f;
        _halfSize = _grid.Size * _grid.SquareSize / 2.0f;
        _gridPosition = _grid.transform.position;

        MoveToCenter();
        _targetPosition = transform.position;
    }

    void Update()
    {
        HandleDragging();
        HandleRotation();
        KeepWithinBounds();

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
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            IsDragging = false;
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

                // Limit the gridPoint to the dimensions of the grid bounds
                gridPoint.x = Mathf.Clamp(gridPoint.x, _gridPosition.x - _halfSize, _gridPosition.x + _halfSize);
                gridPoint.z = Mathf.Clamp(gridPoint.z, _gridPosition.z - _halfSize, _gridPosition.z + _halfSize);

                _targetPosition = gridPoint;
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

    private void MoveToCenter()
    {
        transform.position = _gridPosition + new Vector3(0, transform.position.y, 0);
        _targetPosition = transform.position;
    }

    private void KeepWithinBounds()
    {
        Vector3 clampedPosition = _targetPosition;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, _gridPosition.x - _halfSize, _gridPosition.x + _halfSize);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, _gridPosition.z - _halfSize, _gridPosition.z + _halfSize);

        _targetPosition = clampedPosition;
    }
}
