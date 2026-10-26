using UnityEngine;
using UnityEngine.EventSystems;

public class ShowerHeadDragger : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private ShowerMinigame showerMinigame;
    [SerializeField] private RectTransform hamaTargetRect;

    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        if (IsOverlapping(hamaTargetRect))
        {
            showerMinigame.ProcessScrubDrag(eventData.position);
        }
        else
        {
            showerMinigame.ResetScrubPosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        showerMinigame.ResetScrubPosition();
    }

    private bool IsOverlapping(RectTransform target)
    {
        Vector3[] cornersA = new Vector3[4];
        Vector3[] cornersB = new Vector3[4];

        rectTransform.GetWorldCorners(cornersA);
        target.GetWorldCorners(cornersB);

        Rect rectA = CreateSafeRect(cornersA);
        Rect rectB = CreateSafeRect(cornersB);

        return rectA.Overlaps(rectB);
    }

    private Rect CreateSafeRect(Vector3[] corners)
    {
        float minX = Mathf.Min(corners[0].x, corners[2].x);
        float maxX = Mathf.Max(corners[0].x, corners[2].x);
        float minY = Mathf.Min(corners[0].y, corners[2].y);
        float maxY = Mathf.Max(corners[0].y, corners[2].y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
}