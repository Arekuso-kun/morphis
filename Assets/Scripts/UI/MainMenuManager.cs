using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _settingsMenu;
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private Vector3 _targetCameraPosition = new(0, -1f, -16f);
    [SerializeField] private float _cameraMoveDuration = 2f;

    void Awake()
    {
        if (_mainMenu == null)
        {
            Debug.LogError("Main menu is not assigned!");
            enabled = false;
            return;
        }

        if (_settingsMenu == null)
        {
            Debug.LogError("Settings menu is not assigned!");
            enabled = false;
            return;
        }

        if (_mainCamera == null)
        {
            Debug.LogError("Main camera is not assigned!");
            enabled = false;
            return;
        }

        if (_mainCamera.GetComponent<Parallax>() == null)
        {
            Debug.LogError("Parallax component not found on the main camera.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (_settingsMenu.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
        }
    }

    public void PlayGame()
    {
        _mainMenu.GetComponent<SlideAnimationController>().CloseMainMenu();
        _mainCamera.GetComponent<Parallax>().enabled = false;

        _mainCamera.transform.DOMove(_targetCameraPosition, _cameraMoveDuration)
            .SetEase(Ease.InOutQuad)
            .SetDelay(1f)
            .OnComplete(() =>
            {
                SceneManager.LoadScene("MainScene");
                _mainCamera.GetComponent<Parallax>().enabled = true;
            });
    }

    public void OpenSettings()
    {
        _mainMenu.SetActive(false);
        _settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        _settingsMenu.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game clicked!");
        Application.Quit();
    }
}
