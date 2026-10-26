using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform tooltipRect;

    private Canvas canvas;
    private RectTransform canvasRect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        tooltipObject.SetActive(false);
    }

    public void Show(string message, Vector2 screenPos)
    {
        tooltipText.text = message;
        tooltipObject.SetActive(true);
        SetClampedPosition(screenPos);
    }

    public void UpdatePosition(Vector2 screenPos)
    {
        if (tooltipObject.activeSelf)
        {
            SetClampedPosition(screenPos);
        }
    }

    public void Hide()
    {
        tooltipObject.SetActive(false);
    }

    private void SetClampedPosition(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        Vector2 targetPos = localPoint + new Vector2(0f, 24f);

        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 boxSize = tooltipRect.sizeDelta;

        float minX = -canvasSize.x * 0.5f + (boxSize.x * tooltipRect.pivot.x);
        float maxX = canvasSize.x * 0.5f - (boxSize.x * (1f - tooltipRect.pivot.x));
        float minY = -canvasSize.y * 0.5f + (boxSize.y * tooltipRect.pivot.y);
        float maxY = canvasSize.y * 0.5f - (boxSize.y * (1f - tooltipRect.pivot.y));

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        tooltipRect.anchoredPosition = targetPos;
    }
}