using UnityEngine;
using UnityEngine.UI;

public class ShowerMinigame : MonoBehaviour
{
    [Header("Stages")]
    [SerializeField] private GameObject stage1Container;
    [SerializeField] private GameObject stage2Container;

    [Header("Stage 1: Thermometer Needle")]
    [SerializeField] private RectTransform needle;
    [SerializeField] private float needleSpeed = 160f;
    [SerializeField] private float minX = -45f;
    [SerializeField] private float maxX = 45f;
    [SerializeField] private float sweetZoneMin = -14f;
    [SerializeField] private float sweetZoneMax = 14f;
    private bool needleMoving = false;
    private int needleDirection = 1;

    [Header("Stage 2: Showerhead & Cleaning")]
    [SerializeField] private RectTransform showerHeadRect;
    [SerializeField] private Image dirtLayer;
    [SerializeField] private float totalScrubDistanceGoal = 600f;

    private Vector2 showerHeadDefaultPos;
    private float currentScrubDistance = 0f;
    private Vector2 lastScrubPos = Vector2.zero;
    private float startDirtAlpha = 1f;

    void Awake()
    {
        showerHeadDefaultPos = showerHeadRect.anchoredPosition;
    }

    void OnEnable()
    {
        ResetShowerhead();
        StartStage1();
    }

    void OnDisable()
    {
        ResetShowerhead();
        SoundManager.Instance.StopShowerWater();
    }

    private void ResetShowerhead()
    {
        lastScrubPos = Vector2.zero;
        showerHeadRect.anchoredPosition = showerHeadDefaultPos;
    }

    public void StartStage1()
    {
        stage1Container.SetActive(true);
        stage2Container.SetActive(false);
        needleMoving = true;
        needle.anchoredPosition = new Vector2(minX, needle.anchoredPosition.y);
    }

    void Update()
    {
        if (stage1Container.activeSelf && needleMoving)
        {
            float newX = needle.anchoredPosition.x + (needleDirection * needleSpeed * Time.deltaTime);
            if (newX > maxX) { newX = maxX; needleDirection = -1; }
            else if (newX < minX) { newX = minX; needleDirection = 1; }
            needle.anchoredPosition = new Vector2(newX, needle.anchoredPosition.y);

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                CheckTemperatureStop();
            }
        }
    }

    private void CheckTemperatureStop()
    {
        needleMoving = false;
        float finalX = needle.anchoredPosition.x;

        if (finalX >= sweetZoneMin && finalX <= sweetZoneMax)
        {
            StartStage2();
        }
        else if (finalX < sweetZoneMin)
        {
            GameManager.Instance.TriggerDeath("Burning");
        }
        else if (finalX > sweetZoneMax)
        {
            GameManager.Instance.TriggerDeath("Freezing");
        }
    }

    public void StartStage2()
    {
        stage1Container.SetActive(false);
        stage2Container.SetActive(true);

        NavigationManager.Instance.ShowHamaForShowerStage2();

        currentScrubDistance = 0f;
        lastScrubPos = Vector2.zero;

        int currentHygiene = GameManager.Instance.hygiene;
        startDirtAlpha = (currentHygiene <= 1) ? 1.0f : 0.6f;

        dirtLayer.gameObject.SetActive(true);
        Color c = dirtLayer.color;
        c.a = startDirtAlpha;
        dirtLayer.color = c;
    }

    public void ProcessScrubDrag(Vector2 currentPointerPos)
    {
        if (!stage2Container.activeSelf) return;

        if (lastScrubPos != Vector2.zero)
        {
            float distanceDelta = Vector2.Distance(currentPointerPos, lastScrubPos);

            if (distanceDelta > 0.1f && distanceDelta < 60f)
            {
                SoundManager.Instance.StartShowerWater();
                currentScrubDistance += distanceDelta;
                float progressRatio = Mathf.Clamp01(currentScrubDistance / totalScrubDistanceGoal);

                Color c = dirtLayer.color;
                c.a = Mathf.Lerp(startDirtAlpha, 0f, progressRatio);
                dirtLayer.color = c;

                if (progressRatio >= 1f)
                {
                    FinishCleaning();
                }
            }
            else
            {
                SoundManager.Instance.PauseShowerWater();
            }
        }

        lastScrubPos = currentPointerPos;
    }

    public void ResetScrubPosition()
    {
        lastScrubPos = Vector2.zero;
        SoundManager.Instance.PauseShowerWater();
    }

    private void FinishCleaning()
    {
        SoundManager.Instance.StopShowerWater();
        SoundManager.Instance.PlaySFX(SoundManager.Instance.showerSuccessClip);

        Color c = dirtLayer.color;
        c.a = 0f;
        dirtLayer.color = c;
        dirtLayer.gameObject.SetActive(false);

        GameManager.Instance.ModifyHygiene(6);
        NavigationManager.Instance.ReturnToMainRoom();
    }
}