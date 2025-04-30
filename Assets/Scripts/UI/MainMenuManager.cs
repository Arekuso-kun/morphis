using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _settingsMenu;

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
        SceneManager.LoadScene("MainScene");
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
