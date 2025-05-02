using UnityEngine;
using DG.Tweening;

public class SlideAnimation : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Vector2 _offScreenPosition;
    private Vector2 _onScreenPosition;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _onScreenPosition = _rectTransform.anchoredPosition;
        _offScreenPosition = new Vector2(-_onScreenPosition.x, _onScreenPosition.y);
    }

    private void OnEnable()
    {
        _rectTransform.anchoredPosition = _offScreenPosition;
        _rectTransform.DOAnchorPos(_onScreenPosition, 0.5f)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true);
    }

    public void ClosePanel(System.Action onComplete = null)
    {
        _rectTransform.DOAnchorPos(_offScreenPosition, 0.5f)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    public void ClosePanel()
    {
        ClosePanel(null);
    }
}
