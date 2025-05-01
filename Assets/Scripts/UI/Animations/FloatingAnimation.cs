using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] private float floatStrength = 0.2f;
    [SerializeField] private float floatSpeed = 0.1f;

    [Header("Tilting")]
    [SerializeField] private float tiltStrength = 2f;
    [SerializeField] private float tiltSpeed = 0.1f;

    private Vector3 _startPos;
    private Quaternion _startRot;
    private float _timeOffset;

    void Start()
    {
        _startPos = transform.position;
        _startRot = transform.rotation;
        _timeOffset = Random.Range(0f, 100f);  // desync
    }

    void Update()
    {
        float t = Time.time + _timeOffset;

        float floatOffset = (Mathf.PerlinNoise(t * floatSpeed, 0f) - 0.5f) * 2f * floatStrength;
        transform.position = _startPos + new Vector3(0f, floatOffset, 0f);

        float tiltX = (Mathf.PerlinNoise(t * tiltSpeed, 1f) - 0.5f) * 2f * tiltStrength;
        float tiltZ = (Mathf.PerlinNoise(t * tiltSpeed, 2f) - 0.5f) * 2f * tiltStrength;
        transform.rotation = _startRot * Quaternion.Euler(tiltX, 0f, tiltZ);
    }
}
