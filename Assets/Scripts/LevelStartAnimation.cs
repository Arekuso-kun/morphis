using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class LevelStartAnimation : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;

    [SerializeField] private Vector3 _startTraget = new(0f, -2f, 0f);
    [SerializeField] private float _startDistance = 24f;
    [SerializeField] private float _startTilt = 0f;
    [SerializeField] private float _duration = 4f;
    [SerializeField] private List<GameObject> _toEnable;

    private Vector3 _originalTarget;
    private float _originalDistance;
    private float _originalTilt;

    void Awake()
    {
        if (_cameraController == null)
        {
            Debug.LogWarning("CameraController is not assigned!");
            _cameraController = Camera.main.GetComponent<CameraController>();
        }
    }

    void Start()
    {
        StartCoroutine(PlayAnimationAfterDelay());
    }

    private IEnumerator PlayAnimationAfterDelay()
    {
        _originalTarget = _cameraController.Target;
        _originalDistance = _cameraController.Distance;
        _originalTilt = _cameraController.Tilt;

        _cameraController.LockCameraChanges = true;
        _cameraController.LockUserInput = true;
        CameraController.GlobalInteractionLock = true;

        _cameraController.Target = _startTraget;
        _cameraController.Distance = _startDistance;
        _cameraController.Tilt = _startTilt;

        yield return null;

        Sequence camSequence = DOTween.Sequence();

        camSequence.Append(DOTween.To(
            () => _cameraController.Target,
            x => _cameraController.Target = x,
            _originalTarget,
            _duration
        ).SetEase(Ease.InOutQuad));

        camSequence.Join(DOTween.To(
            () => _cameraController.Distance,
            x => _cameraController.Distance = x,
            _originalDistance,
            _duration
        ).SetEase(Ease.InOutQuad));

        camSequence.Join(DOTween.To(
            () => _cameraController.Tilt,
            x => _cameraController.Tilt = x,
            _originalTilt,
            _duration
        ).SetEase(Ease.InOutQuad));

        camSequence.OnComplete(() =>
        {
            _cameraController.LockCameraChanges = false;
            _cameraController.LockUserInput = false;
            CameraController.GlobalInteractionLock = false;

            _cameraController.SetOriginalCameraState();

            foreach (GameObject obj in _toEnable)
            {
                obj.SetActive(true);
            }
        });
    }
}
