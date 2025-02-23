using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridSnap : MonoBehaviour
{
    [Tooltip("The grid object for reference")]
    public GenerateGrid grid;

    private float gridSize;
    private Camera mainCamera;
    private bool isDragging = false;
    private float halfSize;
    private Vector3 gridPosotion;
    private bool isRotating = false;

    void Start()
    {
        if (grid == null)
        {
            Debug.LogError("Grid object is missing!");
            return;
        }

        mainCamera = Camera.main;
        gridSize = grid.squareSize / 2.0f;
        halfSize = grid.size * grid.squareSize / 2.0f;
        gridPosotion = grid.transform.position;

        MoveToCenter();
    }

    void Update()
    {
        if (grid == null) return;

        HandleDragging();
        HandleRotation();
        KeepWithinBounds();
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                Vector3 gridPoint = new(
                    Mathf.Round(point.x / gridSize) * gridSize,
                    transform.position.y,
                    Mathf.Round(point.z / gridSize) * gridSize
                );

                // Limit the gridPoint to the dimensions of the grid bounds
                gridPoint.x = Mathf.Clamp(gridPoint.x, gridPosotion.x - halfSize, gridPosotion.x + halfSize);
                gridPoint.z = Mathf.Clamp(gridPoint.z, gridPosotion.z - halfSize, gridPosotion.z + halfSize);

                transform.position = gridPoint;
            }
        }
    }

    private void HandleRotation()
    {
        if (isRotating) return;

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
        isRotating = true;
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
        isRotating = false;
    }

    private void MoveToCenter()
    {
        transform.position = gridPosotion + new Vector3(0, transform.position.y, 0);
    }

    private void KeepWithinBounds()
    {
        Vector3 position = transform.position;

        if (position.x < gridPosotion.x - halfSize || position.x > gridPosotion.x + halfSize ||
            position.z < gridPosotion.z - halfSize || position.z > gridPosotion.z + halfSize)
        {
            MoveToCenter();
        }
    }
}
