using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePoop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private RectTransform hamaRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        GameObject hama = GameObject.FindWithTag("Pet");
        hamaRect = hama.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        TrashCanController.Instance.Show();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        bool overTrash = TrashCanController.Instance.IsOverTrash(rectTransform);
        TrashCanController.Instance.SetOpenState(overTrash);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (TrashCanController.Instance.IsOverTrash(rectTransform))
        {
            TrashCanController.Instance.Hide();
            GameManager.Instance.UnregisterPoop(gameObject);

            SoundManager.Instance.PlaySFX(SoundManager.Instance.trashDumpClip);

            Destroy(gameObject);
            return;
        }

        if (IsOverlapping(hamaRect))
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.eatingClip);

            GameManager.Instance.FeedPoopToHama();
            TrashCanController.Instance.Hide();
            GameManager.Instance.UnregisterPoop(gameObject);

            Destroy(gameObject);
            return;
        }

        TrashCanController.Instance.Hide();
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