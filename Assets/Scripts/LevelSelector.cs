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
        for (int i = 1; i <= levelCount; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, gridParent);
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();

            string levelName = $"Level_{i:00}";
            buttonText.text = $"{i}";

            button.onClick.AddListener(() => SceneManager.LoadScene(levelName));
        }
    }
}
