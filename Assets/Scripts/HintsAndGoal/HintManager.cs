using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private GameObject _objectsContainer;
    [SerializeField] private GameObject _hintsContainer;
    [SerializeField] private GameObject _hintNavigator;

    void Awake()
    {
        if (_objectsContainer == null)
        {
            Debug.LogError("Objects container is not assigned!");
            enabled = false;
            return;
        }

        if (_hintsContainer == null)
        {
            Debug.LogError("Hints container is not assigned!");
            enabled = false;
            return;
        }

        if (_hintNavigator == null)
        {
            Debug.LogError("Hint navigator is not assigned!");
            enabled = false;
            return;
        }
    }

    public void ShowMainObjects()
    {
        _objectsContainer.SetActive(true);
        _hintsContainer.SetActive(false);
        _hintNavigator.GetComponent<HintNavigatorAnimation>().CloseNavigator();
    }

    public void ShowHints()
    {
        _objectsContainer.SetActive(false);
        _hintsContainer.SetActive(true);
        _hintNavigator.SetActive(true);
    }
}
