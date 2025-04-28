using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;

    void Awake()
    {
        if (pauseMenu == null)
        {
            Debug.LogError("Pause menu is not assigned!");
            enabled = false;
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        Time.timeScale = pauseMenu.activeSelf ? 0 : 1;
    }
}
