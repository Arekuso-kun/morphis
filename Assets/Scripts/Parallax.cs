using UnityEngine;
using DG.Tweening;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float _parallaxStrength = 0.5f;
    [SerializeField] private float _duration = 0.3f;

    private Vector3 _startPosition;

    void Awake()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        float x = (mousePos.x / Screen.width - 0.5f) * 2f;
        float y = (mousePos.y / Screen.height - 0.5f) * 2f;

        Vector3 targetOffset = new Vector3(x, y, 0) * _parallaxStrength;
        Vector3 targetPos = _startPosition + targetOffset;

        transform.DOMove(targetPos, _duration).SetEase(Ease.OutQuad);
    }
}
