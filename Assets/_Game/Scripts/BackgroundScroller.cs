using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float baseScrollSpeed = 60f;
    [SerializeField] private float resetDistance = 280f;
    [SerializeField] private bool useManualStartCoordinate = false;
    [SerializeField] private Vector2 manualStartPosition = new Vector2(211f, 0f);

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private bool hasRecordedStart = false;

    void Awake()
    {
        EnsureInitialized();
    }

    void OnEnable()
    {
        EnsureInitialized();
        ResetToStart();
    }

    private void EnsureInitialized()
    {
        if (hasRecordedStart) return;

        rectTransform = GetComponent<RectTransform>();

        if (useManualStartCoordinate)
        {
            startPosition = manualStartPosition;
        }
        else
        {
            startPosition = rectTransform.anchoredPosition;
        }

        hasRecordedStart = true;
    }

    public void ResetToStart()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = startPosition;
    }

    void Update()
    {
        if (!RunnerGameManager.Instance.IsRunning) return;

        float moveDelta = baseScrollSpeed * RunnerGameManager.Instance.SpeedMultiplier * Time.deltaTime;
        rectTransform.anchoredPosition += Vector2.left * moveDelta;

        if (rectTransform.anchoredPosition.x <= startPosition.x - resetDistance)
        {
            float overshot = (startPosition.x - resetDistance) - rectTransform.anchoredPosition.x;
            rectTransform.anchoredPosition = new Vector2(startPosition.x - overshot, startPosition.y);
        }
    }
}