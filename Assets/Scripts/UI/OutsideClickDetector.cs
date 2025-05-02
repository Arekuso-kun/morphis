using UnityEngine;
using UnityEngine.EventSystems;

public class OutsideClickDetector : MonoBehaviour
{
    void Awake()
    {
        if (gameObject.GetComponent<SlideAnimation>() == null)
        {
            Debug.LogError("SlideAnimation component is missing on the GameObject.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI())
            {
                gameObject.GetComponent<SlideAnimation>().ClosePanel();
            }
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
