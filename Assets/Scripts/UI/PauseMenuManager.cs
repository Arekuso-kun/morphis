using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;

    private bool _isPaused = false;
    private PauseMenuAnimation _pauseMenuAnimation;

    void Awake()
    {
        if (_pauseMenu == null)
        {
            Debug.LogError("Pause menu is not assigned!");
            enabled = false;
            return;
        }

        _pauseMenuAnimation = _pauseMenu.GetComponent<PauseMenuAnimation>();
        if (_pauseMenuAnimation == null)
        {
            Debug.LogError("PauseMenuAnimation script not found on the pause menu GameObject.");
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

    private void OpenMenu()
    {
        _isPaused = true;
        _pauseMenu.SetActive(true); // will trigger OnEnable 
        Time.timeScale = 0f;
    }

    private void CloseMenu()
    {
        _isPaused = false;
        _pauseMenuAnimation.CloseMenu();
        Time.timeScale = 1f;
    }
}
