using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip UI References")]
    [SerializeField] private RectTransform tooltipBox;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Tooltip Settings")]
    [SerializeField] private Vector2 padding = new(20f, 10f);

    public enum TooltipPosition { Above, Below }
    [SerializeField] private TooltipPosition position = TooltipPosition.Above;
    [SerializeField] private float verticalOffset = 50f;

    private RectTransform buttonRect;
    private RectTransform canvasRect;

    void Start()
    {
        tooltipBox.gameObject.SetActive(false);
        buttonRect = GetComponent<RectTransform>();
        canvasRect = tooltipBox.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        tooltipBox.gameObject.SetActive(true);

        Vector2 newSize = new(tooltipText.preferredWidth + padding.x, tooltipText.preferredHeight + padding.y);
        tooltipBox.sizeDelta = newSize;

        Vector3[] buttonCorners = new Vector3[4];
        buttonRect.GetWorldCorners(buttonCorners);

        Vector3 worldPosition;
        if (position == TooltipPosition.Above)
        {
            worldPosition = (buttonCorners[1] + buttonCorners[2]) / 2f; // top center
            worldPosition.y += verticalOffset;
        }
        else
        {
            worldPosition = (buttonCorners[0] + buttonCorners[3]) / 2f; // bottom center
            worldPosition.y -= verticalOffset;
        }

        tooltipBox.position = worldPosition;

        ClampTooltipToCanvas();
    }

    private void ClampTooltipToCanvas()
    {
        Vector3[] tooltipCorners = new Vector3[4];
        Vector3[] canvasCorners = new Vector3[4];

        tooltipBox.GetWorldCorners(tooltipCorners);
        canvasRect.GetWorldCorners(canvasCorners);

        Vector3 canvasMin = canvasCorners[0]; // bottom left
        Vector3 canvasMax = canvasCorners[2]; // top right

        Vector3 offset = Vector3.zero;

        foreach (Vector3 corner in tooltipCorners)
        {
            if (corner.x < canvasMin.x)
                offset.x += canvasMin.x - corner.x;
            if (corner.x > canvasMax.x)
                offset.x -= corner.x - canvasMax.x;

            if (corner.y < canvasMin.y)
                offset.y += canvasMin.y - corner.y;
            if (corner.y > canvasMax.y)
                offset.y -= corner.y - canvasMax.y;
        }

        tooltipBox.position += offset;
    }

    private void HideTooltip()
    {
        tooltipBox.gameObject.SetActive(false);
    }
}
