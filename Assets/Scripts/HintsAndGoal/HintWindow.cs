using UnityEngine;

public class HintWindow : MonoBehaviour
{
    public void ToggleHintWindow()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
