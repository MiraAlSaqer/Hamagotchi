using UnityEngine;

public class TrashCanController : MonoBehaviour
{
    public static TrashCanController Instance { get; private set; }

    [Header("Trash Can Graphics")]
    [SerializeField] private GameObject closedCanObject;
    [SerializeField] private GameObject openCanObject;

    private RectTransform rectTransform;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetOpenState(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetOpenState(bool isOpen)
    {
        closedCanObject.SetActive(!isOpen);
        openCanObject.SetActive(isOpen);
    }

    public bool IsOverTrash(RectTransform itemRect)
    {
        Vector3[] cornersA = new Vector3[4];
        Vector3[] cornersB = new Vector3[4];

        rectTransform.GetWorldCorners(cornersA);
        itemRect.GetWorldCorners(cornersB);

        Rect rectA = new Rect(cornersA[0], cornersA[2] - cornersA[0]);
        Rect rectB = new Rect(cornersB[0], cornersB[2] - cornersB[0]);

        return rectA.Overlaps(rectB);
    }
}