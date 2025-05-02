using UnityEngine;

[System.Serializable]
public class ObjectState
{
    public string meshFileName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 colliderSize;
    public TransformationMode mode;
    public bool isGoal;
}