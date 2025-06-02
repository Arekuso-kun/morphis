using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _settingsMenu;
    [SerializeField] private GameObject _settingsWindow;
    [SerializeField] private GameObject _controlsWindow;
    [SerializeField] private GameObject _levelSelectorMenu;
    [SerializeField] private GameObject _levelSelectorWindow;
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private Vector3 _targetCameraPosition = new(0, -1f, -16f);
    [SerializeField] private float _cameraMoveDuration = 2f;

    private MenuBackgroundAnimation _settingsMenuAnimation;
    private MenuSlideAnimation _settingsWindowAnimation;
    private MenuSlideAnimation _controlsWindowAnimation;
    private MenuBackgroundAnimation _levelSelectorMenuAnimation;
    private MenuSlideAnimation _levelSelectorWindowAnimation;

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

        if (_settingsWindow == null)
        {
            Debug.LogError("Settings window is not assigned!");
            enabled = false;
            return;
        }

        if (_controlsWindow == null)
        {
            Debug.LogError("Controls window is not assigned!");
            enabled = false;
            return;
        }

        if (_levelSelectorMenu == null)
        {
            Debug.LogError("Level selector menu is not assigned!");
            enabled = false;
            return;
        }

        if (_levelSelectorWindow == null)
        {
            Debug.LogError("Level selector window is not assigned!");
            enabled = false;
            return;
        }

        _settingsMenuAnimation = _settingsMenu.GetComponent<MenuBackgroundAnimation>();
        if (_settingsMenuAnimation == null)
        {
            Debug.LogError("MenuBackgroundAnimation script not found on the settings menu GameObject.");
            enabled = false;
            return;
        }

        _settingsWindowAnimation = _settingsWindow.GetComponent<MenuSlideAnimation>();
        if (_settingsWindowAnimation == null)
        {
            Debug.LogError("MenuSlideAnimation script not found on the settings window GameObject.");
            enabled = false;
            return;
        }

        _controlsWindowAnimation = _controlsWindow.GetComponent<MenuSlideAnimation>();
        if (_controlsWindowAnimation == null)
        {
            Debug.LogError("MenuSlideAnimation script not found on the controls window GameObject.");
            enabled = false;
            return;
        }
        _controlsWindowAnimation.Direction = MenuSlideAnimation.SlideDirection.Down;
        _controlsWindowAnimation.Speed = MenuSlideAnimation.SlideSpeed.Fast;

        _levelSelectorMenuAnimation = _levelSelectorMenu.GetComponent<MenuBackgroundAnimation>();
        if (_levelSelectorMenuAnimation == null)
        {
            Debug.LogError("MenuBackgroundAnimation script not found on the level selector menu GameObject.");
            enabled = false;
            return;
        }

        _levelSelectorWindowAnimation = _levelSelectorWindow.GetComponent<MenuSlideAnimation>();
        if (_levelSelectorWindowAnimation == null)
        {
            Debug.LogError("MenuSlideAnimation script not found on the level selector window GameObject.");
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_settingsMenu.activeSelf)
            {
                if (_controlsWindow.activeSelf)
                {
                    CloseControlsWindow();
                }
                else if (_settingsWindow.activeSelf)
                {
                    CloseSettings();
                }
            }
            else if (_levelSelectorMenu.activeSelf)
            {
                CloseLevelSelector();
            }
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
        _settingsMenu.SetActive(true);
        _settingsWindowAnimation.Direction = MenuSlideAnimation.SlideDirection.Down;
        _settingsWindowAnimation.Speed = MenuSlideAnimation.SlideSpeed.Slow;
        _settingsWindow.SetActive(true);
        _controlsWindow.SetActive(false);
    }

    public void CloseSettings()
    {
        _settingsMenuAnimation.CloseMenu();
        _settingsWindowAnimation.Direction = MenuSlideAnimation.SlideDirection.Down;
        _settingsWindowAnimation.Speed = MenuSlideAnimation.SlideSpeed.Slow;
        _settingsWindowAnimation.CloseMenu();
    }

    public void OpenControlsWindow()
    {
        _settingsWindowAnimation.Direction = MenuSlideAnimation.SlideDirection.Up;
        _settingsWindowAnimation.Speed = MenuSlideAnimation.SlideSpeed.Fast;

        _settingsWindowAnimation.CloseMenu();
        _controlsWindow.SetActive(true);
    }

    public void CloseControlsWindow()
    {
        _settingsWindowAnimation.Direction = MenuSlideAnimation.SlideDirection.Up;
        _settingsWindowAnimation.Speed = MenuSlideAnimation.SlideSpeed.Fast;

        _controlsWindowAnimation.CloseMenu();
        _settingsWindow.SetActive(true);
    }

    public void OpenLevelSelector()
    {
        _levelSelectorMenu.SetActive(true);
    }

    public void CloseLevelSelector()
    {
        _levelSelectorMenuAnimation.CloseMenu();
        _levelSelectorWindowAnimation.CloseMenu();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game clicked!");
        Application.Quit();
    }
}
