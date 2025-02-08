using UnityEngine;

public class MoveAboveGrid : MonoBehaviour
{
    public void AdjustHeightAboveGrid(float gridHeight, float gridHeightOffset)
    {
        float halfHeight = GetComponent<Renderer>().bounds.size.y / 2.0f;
        float newY = gridHeight + gridHeightOffset + halfHeight;

        if (transform.position.y != newY)
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
