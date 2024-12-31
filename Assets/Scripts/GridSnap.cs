using Unity.VisualScripting;
using UnityEngine;

public class GridSnap : MonoBehaviour
{
    [Tooltip("Size of the grid squares")]
    public float gridSize = 0.5f;

    [Tooltip("The grid object for reference")]
    public Grid grid;

    private Camera mainCamera;
    private bool isDragging = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        float halfSize = grid.size * grid.squareSize / 2.0f;
        Vector3 checkerboardPosition = grid.transform.position;

        // Check if the object is being dragged
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
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
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);
                Vector3 gridPoint = new Vector3(
                    Mathf.Round(point.x / gridSize) * gridSize,
                    transform.position.y,
                    Mathf.Round(point.z / gridSize) * gridSize
                );

                // Limit the gridPoint to the dimensions of the checkerboard
                gridPoint.x = Mathf.Clamp(gridPoint.x, checkerboardPosition.x - halfSize, checkerboardPosition.x + halfSize);
                gridPoint.z = Mathf.Clamp(gridPoint.z, checkerboardPosition.z - halfSize, checkerboardPosition.z + halfSize);

                transform.position = gridPoint;
            }
        }

        float halfHeight = GetComponent<Renderer>().bounds.size.y / 2.0f;

        // Check if the object is outside the grid and move it to the center if it is
        if (transform.position.x < checkerboardPosition.x - halfSize || transform.position.x > checkerboardPosition.x + halfSize ||
            transform.position.z < checkerboardPosition.z - halfSize || transform.position.z > checkerboardPosition.z + halfSize)
        {
            transform.position = checkerboardPosition + new Vector3(0, halfHeight, 0);
        }
    }

    public bool IsDragging()
    {
        return isDragging;
    }
}
