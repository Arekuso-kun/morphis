using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SlideAnimationController : MonoBehaviour
{
    [SerializeField] private GameObject _buttonContainer;
    [SerializeField] private CanvasGroup _panel;
    [SerializeField] private float _duration = 0.3f;

    private void Awake()
    {
        if (_buttonContainer == null)
        {
            Debug.LogError("Button container is not assigned!");
            enabled = false;
            return;
        }

        if (_buttonContainer.GetComponent<VerticalLayoutGroup>() == null)
        {
            Debug.LogError("Button container does not have a VerticalLayoutGroup component!");
            enabled = false;
            return;
        }

        if (_panel == null)
        {
            Debug.LogError("Panel is not assigned!");
            enabled = false;
            return;
        }

        if (_panel.GetComponent<RectTransform>() == null)
        {
            Debug.LogError("Panel does not have a RectTransform component!");
            enabled = false;
            return;
        }
    }

    public void CloseMainMenu()
    {
        _buttonContainer.GetComponent<VerticalLayoutGroup>().enabled = false;

        var sequence = DOTween.Sequence();
        foreach (Transform button in _buttonContainer.transform)
        {
            sequence.Append(button.DOLocalMoveX(-400, _duration).SetEase(Ease.InCirc));
        }
        sequence.Append(_panel.DOFade(0, _duration));

        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
