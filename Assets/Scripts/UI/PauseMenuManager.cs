using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _pauseWindow;

    private bool _isPaused = false;
    private MenuBackgroundAnimation _pauseMenuAnimation;
    private MenuSlideAnimation _pauseWindowAnimation;

    void Awake()
    {
        if (_pauseMenu == null)
        {
            Debug.LogError("Pause menu is not assigned!");
            enabled = false;
            return;
        }

        if (_pauseWindow == null)
        {
            Debug.LogError("Pause window is not assigned!");
            enabled = false;
            return;
        }

        _pauseMenuAnimation = _pauseMenu.GetComponent<MenuBackgroundAnimation>();
        if (_pauseMenuAnimation == null)
        {
            Debug.LogError("PauseMenuAnimation script not found on the pause menu GameObject.");
            enabled = false;
            return;
        }

        _pauseWindowAnimation = _pauseWindow.GetComponent<MenuSlideAnimation>();
        if (_pauseWindowAnimation == null)
        {
            Debug.LogError("PauseWindowAnimation script not found on the pause window GameObject.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isPaused)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }
    }

    public void OpenMenu()
    {
        _isPaused = true;
        _pauseMenu.SetActive(true); // will trigger OnEnable 
        _pauseWindow.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        _isPaused = false;
        _pauseMenuAnimation.CloseMenu();
        _pauseWindowAnimation.CloseMenu();
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }
}
