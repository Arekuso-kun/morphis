using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public GameObject levelButtonPrefab;
    public Transform gridParent;
    public int levelCount = 20;

    void Start()
    {
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);

        for (int i = 1; i <= levelCount; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, gridParent);
            Button button = buttonObj.GetComponentInChildren<Button>();
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();

            string levelName = $"Level_{i:00}";
            buttonText.text = $"{i}";

            if (i <= highestUnlocked)
            {
                button.onClick.AddListener(() => SceneManager.LoadScene(levelName));
            }
            else
            {
                button.interactable = false;
            }
        }
    }
}
