using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class GridSnap : MonoBehaviour
{
    [Tooltip("The grid object for reference")]
    public Grid grid;

    private float gridSize;
    private Camera mainCamera;
    private bool isDragging = false;
    private float halfSize;
    private Vector3 gridPosotion;

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
        // TO DO: Animate the rotation

        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.Rotate(Vector3.right, -90f, Space.Self);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.Rotate(Vector3.right, 90f, Space.Self);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -90f, Space.Self);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.Rotate(Vector3.up, 90f, Space.Self);
        }
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
