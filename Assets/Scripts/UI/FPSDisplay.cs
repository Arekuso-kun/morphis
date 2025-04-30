using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    private TMP_Text _fpsText;
    private float _deltaTime = 0.0f;

    void Awake()
    {
        _fpsText = GetComponent<TMP_Text>();
        if (_fpsText == null)
        {
            Debug.LogError("FPS Text component not assigned!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

        float msec = _deltaTime * 1000.0f;
        float fps = 1.0f / _deltaTime;
        _fpsText.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
    }
}
