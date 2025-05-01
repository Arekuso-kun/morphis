using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class LevelStartAnimation : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Vector3 _startPosition = new(0, -2f, -24f);
    [SerializeField] private Vector3 _startRotation = Vector3.zero;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private List<GameObject> _toEnable;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    void Awake()
    {
        if (_mainCamera == null)
        {
            Debug.LogWarning("Main camera is not assigned!");
            _mainCamera = Camera.main;
        }
    }

    void Start()
    {
        StartCoroutine(PlayAnimationAfterDelay());
    }

    private IEnumerator PlayAnimationAfterDelay()
    {

        _originalPosition = _mainCamera.transform.position;
        _originalRotation = _mainCamera.transform.rotation;

        _mainCamera.transform.position = _startPosition;
        _mainCamera.transform.rotation = Quaternion.Euler(_startRotation);

        yield return null;

        Sequence camSequence = DOTween.Sequence();

        camSequence.Join(_mainCamera.transform.DOMove(_originalPosition, _duration).SetEase(Ease.InOutQuad));
        camSequence.Join(_mainCamera.transform.DORotateQuaternion(_originalRotation, _duration).SetEase(Ease.InOutQuad));

        camSequence.OnComplete(() =>
        {
            foreach (GameObject obj in _toEnable)
            {
                obj.SetActive(true);
            }
        });
    }
}
