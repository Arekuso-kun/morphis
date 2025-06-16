using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;

    void Awake()
    {
        if (_canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component not assigned!");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.gameObject.SetActive(true);
    }

    void OnEnable()
    {
        Sequence fadeSequence = DOTween.Sequence();
        fadeSequence.Insert(0f, _canvasGroup.DOFade(1f, 1f).SetEase(Ease.InOutSine));
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}