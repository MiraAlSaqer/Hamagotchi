using UnityEngine;
using UnityEngine.UI;

public class DisciplineController : MonoBehaviour
{
    public static DisciplineController Instance { get; private set; }

    [Header("UI & Timing")]
    [SerializeField] private Button disciplineButton;
    [SerializeField] private float windowDuration = 10f;

    [Header("Misbehavior Props")]
    [SerializeField] private GameObject[] misbehaviorObjects;

    public bool isMisbehaving = false;
    public int totalMisbehaviorsSpawned = 0;
    public int totalCorrectDisciplines = 0;
    public int unjustDisciplinesCount = 0;
    public int dailyDisciplinesCount = 0;

    private float misbehaviorTimer = 0f;
    private float nextMisbehaviorInterval = 0f;
    private float activeWindowTimer = 0f;
    private GameObject currentActiveMark = null;

    private const int MaxUnjustDisciplines = 3;
    private const int MaxDailyDisciplines = 5;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        HideAllMarks();
        bool isChild = GameManager.Instance.currentStage == LifeStage.Child;
        disciplineButton.interactable = isChild;
    }

    void Start()
    {
        disciplineButton.onClick.AddListener(OnDisciplinePressed);
        ResetNextInterval();
    }

    void Update()
    {
        if (GameManager.Instance.currentStage != LifeStage.Child || GameManager.Instance.isSleeping) return;

        if (isMisbehaving)
        {
            activeWindowTimer -= Time.deltaTime;
            if (activeWindowTimer <= 0f)
            {
                OnMisbehaviorIgnored();
            }
            return;
        }

        if (NavigationManager.Instance.IsInMainRoom)
        {
            misbehaviorTimer += Time.deltaTime;
            if (misbehaviorTimer >= nextMisbehaviorInterval)
            {
                misbehaviorTimer = 0f;
                ResetNextInterval();
                TriggerMisbehavior();
            }
        }
    }

    private void ResetNextInterval()
    {
        bool testMode = GameManager.Instance.acceleratedTestMode;
        nextMisbehaviorInterval = testMode ? Random.Range(4f, 6f) : Random.Range(240f, 360f);
    }

    private void TriggerMisbehavior()
    {
        isMisbehaving = true;
        activeWindowTimer = windowDuration;
        totalMisbehaviorsSpawned++;

        bool isVisualEvent = (misbehaviorObjects.Length > 0 && Random.value > 0.5f);

        if (isVisualEvent)
        {
            HideAllMarks();
            int randomIndex = Random.Range(0, misbehaviorObjects.Length);
            currentActiveMark = misbehaviorObjects[randomIndex];
            currentActiveMark.SetActive(true);

            SoundManager.Instance.PlaySFX(SoundManager.Instance.misbehaviorPopClip);
        }
        else
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.childMisbehavingClip);
        }
    }

    private void OnMisbehaviorIgnored()
    {
        isMisbehaving = false;
        HideAllMarks();
    }

    public void OnDisciplinePressed()
    {
        if (GameManager.Instance.currentStage != LifeStage.Child)
        {
            disciplineButton.interactable = false;
            return;
        }

        SoundManager.Instance.PlaySFX(SoundManager.Instance.whipDisciplineClip);
        dailyDisciplinesCount++;
        GameManager.Instance.ModifyHappiness(-2);

        if (isMisbehaving)
        {
            isMisbehaving = false;
            totalCorrectDisciplines++;
            HideAllMarks();
        }
        else
        {
            unjustDisciplinesCount++;
        }

        if (unjustDisciplinesCount >= MaxUnjustDisciplines || dailyDisciplinesCount > MaxDailyDisciplines)
        {
            GameManager.Instance.TriggerRunawayEnding();
        }
    }

    public void HideAllMarks()
    {
        for (int i = 0; i < misbehaviorObjects.Length; i++)
        {
            misbehaviorObjects[i].SetActive(false);
        }
        currentActiveMark = null;
    }

    public void OnNewDay()
    {
        dailyDisciplinesCount = 0;
    }

    public bool IsUnderDisciplined()
    {
        int ignored = totalMisbehaviorsSpawned - totalCorrectDisciplines;
        return ignored >= 4 || (totalMisbehaviorsSpawned > 0 && (float)totalCorrectDisciplines / totalMisbehaviorsSpawned < 0.5f);
    }
}