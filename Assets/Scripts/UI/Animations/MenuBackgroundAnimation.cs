using UnityEngine;
using DG.Tweening;

public class MenuBackgroundAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup _background;

    void Awake()
    {
        if (_background == null)
        {
            Debug.LogWarning("Background CanvasGroup is not assigned!");
            return;
        }
    }

    private void OnEnable()
    {
        _background.alpha = 0;
        _background.DOFade(1f, 0.5f)
            .SetUpdate(true);
    }

    public void CloseMenu()
    {
        _background.DOFade(0f, 0.5f)
            .SetUpdate(true)
            .OnComplete(OnComplete);
    }

    private void OnComplete()
    {
        gameObject.SetActive(false);
    }
}
