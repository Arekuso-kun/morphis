using UnityEngine;

public static class MoveUtility
{
    public static void AdjustHeightAboveGrid(GameObject obj, float gridHeight, float gridHeightOffset)
    {
        Renderer renderer = obj.GetComponent<Renderer>();

        float halfHeight = renderer.bounds.size.y / 2.0f;
        float newY = gridHeight + gridHeightOffset + halfHeight;

        if (obj.transform.position.y != newY)
            obj.transform.position = new Vector3(obj.transform.position.x, newY, obj.transform.position.z);
    }
}
