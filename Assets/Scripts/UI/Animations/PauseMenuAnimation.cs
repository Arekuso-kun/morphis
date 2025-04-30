using UnityEngine;
using DG.Tweening;

public class PauseMenuAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform _window;
    [SerializeField] private CanvasGroup _background;

    private Vector2 _offScreenPosition;
    private Vector2 _onScreenPosition;

    void Awake()
    {
        if (_window == null)
        {
            Debug.LogError("Window RectTransform is not assigned!");
            enabled = false;
            return;
        }

        if (_background == null)
        {
            Debug.LogError("Background CanvasGroup is not assigned!");
            enabled = false;
            return;
        }

        _onScreenPosition = _window.anchoredPosition;
        _offScreenPosition = new Vector2(0, -(Screen.height / 2 + _window.rect.height));
    }

    private void OnEnable()
    {
        _background.alpha = 0;
        _background.DOFade(1f, 0.5f)
            .SetUpdate(true);

        _window.anchoredPosition = _offScreenPosition;
        _window.DOAnchorPos(_onScreenPosition, 0.5f)
            .SetEase(Ease.OutExpo)
            .SetDelay(0.1f)
            .SetUpdate(true);
    }

    public void CloseMenu()
    {
        _background.DOFade(0f, 0.5f)
            .SetUpdate(true);

        _window.DOAnchorPos(_offScreenPosition, 0.5f)
            .SetEase(Ease.InExpo)
            .SetUpdate(true)
            .OnComplete(OnComplete);
    }

    private void OnComplete()
    {
        gameObject.SetActive(false);
    }
}
