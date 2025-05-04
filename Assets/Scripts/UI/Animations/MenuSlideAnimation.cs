using UnityEngine;
using DG.Tweening;

public class MenuSlideAnimation : MonoBehaviour
{
    public SlideDirection Direction = SlideDirection.Down;
    public SlideSpeed Speed = SlideSpeed.Slow;

    public enum SlideDirection { Up, Down }
    public enum SlideSpeed { Slow, Fast }

    private RectTransform _rectTransform;
    private Vector2 _offScreenPosition;
    private Vector2 _onScreenPosition;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _onScreenPosition = _rectTransform.anchoredPosition;
        _offScreenPosition = GetOffScreenPosition();
    }

    private void OnEnable()
    {
        _offScreenPosition = GetOffScreenPosition();
        _rectTransform.anchoredPosition = _offScreenPosition;
        _rectTransform.DOAnchorPos(_onScreenPosition, 0.5f)
            .SetEase(Ease.OutExpo)
            .SetDelay(0.1f)
            .SetUpdate(true);
    }

    public void CloseMenu()
    {
        Ease easeType = Ease.InExpo;
        if (Speed == SlideSpeed.Fast) easeType = Ease.OutExpo;

        _offScreenPosition = GetOffScreenPosition();
        _rectTransform.DOAnchorPos(_offScreenPosition, 0.5f)
            .SetEase(easeType)
            .SetUpdate(true)
            .OnComplete(OnComplete);
    }

    private void OnComplete()
    {
        gameObject.SetActive(false);
    }

    private Vector2 GetOffScreenPosition()
    {
        float offsetY = Screen.height / 2 + _rectTransform.rect.height + 50f;

        if (Direction == SlideDirection.Down)
            return new Vector2(0, -offsetY);
        else
            return new Vector2(0, offsetY);
    }
}
