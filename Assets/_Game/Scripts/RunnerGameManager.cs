using TMPro;
using UnityEngine;

public class RunnerGameManager : MonoBehaviour
{
    public static RunnerGameManager Instance { get; private set; }

    [Header("Runner State")]
    public bool IsRunning = false;
    public float SpeedMultiplier = 1f;

    [Header("UI Elements")]
    [SerializeField] private GameObject startPromptText;
    [SerializeField] private TextMeshProUGUI runningScoreText;
    [SerializeField] private TextMeshProUGUI highScoreNumberText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI payoutCoinsText;

    [Header("Game References")]
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private RunnerHama hamaPlayer;

    private float score = 0f;
    private int highScore = 0;
    private int lastCoinMilestone = 0;
    private int coinsAccumulatedThisRun = 0;
    private bool canClickToExit = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        PrepareStartScreen();
    }

    public void PrepareStartScreen()
    {
        IsRunning = false;
        canClickToExit = false;
        SpeedMultiplier = 1f;
        score = 0f;
        lastCoinMilestone = 0;
        coinsAccumulatedThisRun = 0;

        startPromptText.SetActive(true);
        gameOverPanel.SetActive(false);

        highScore = PlayerPrefs.GetInt("RunnerHighScore", 0);
        UpdateDisplays();

        hamaPlayer.ResetPosition();
        spawner.StopSpawning();

        GetComponentInChildren<BackgroundScroller>().ResetToStart();
    }

    void Update()
    {
        if (!IsRunning && startPromptText.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartRun();
            }
            return;
        }

        if (IsRunning)
        {
            score += 15f * SpeedMultiplier * Time.deltaTime;
            SpeedMultiplier += 0.015f * Time.deltaTime;

            bool isChild = GameManager.Instance.currentStage == LifeStage.Child;
            int currentStep = Mathf.FloorToInt(score);

            if (currentStep - lastCoinMilestone >= 100)
            {
                lastCoinMilestone = currentStep;
                if (!isChild)
                {
                    coinsAccumulatedThisRun++;
                    GameManager.Instance.AddCoins(1);
                }
            }

            UpdateDisplays();
        }

        if (canClickToExit && Input.GetMouseButtonDown(0))
        {
            SoundManager.Instance.PlayBackOrPage();
            NavigationManager.Instance.ReturnToMainRoom();
        }
    }

    private void StartRun()
    {
        IsRunning = true;
        startPromptText.SetActive(false);
        spawner.StartSpawning();
    }

    private void UpdateDisplays()
    {
        int scoreInt = Mathf.FloorToInt(score);
        runningScoreText.text = scoreInt.ToString("D5");
        highScoreNumberText.text = Mathf.Max(highScore, scoreInt).ToString("D5");
    }

    public void TriggerGameOver()
    {
        IsRunning = false;
        spawner.StopSpawning();

        SoundManager.Instance.PlaySFX(SoundManager.Instance.runnerGameOverClip);

        int finalScore = Mathf.FloorToInt(score);
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("RunnerHighScore", highScore);
            PlayerPrefs.Save();
        }

        bool isChild = GameManager.Instance.currentStage == LifeStage.Child;

        payoutCoinsText.text = isChild ? "TOO BAD!" : $"{coinsAccumulatedThisRun} COINS!";
        gameOverPanel.SetActive(true);

        GameManager.Instance.ModifyHappiness(6);
        GameManager.Instance.ModifyEnergy(-1);
        GameManager.Instance.ModifyWeight(isChild ? -0.8f : -1.5f);

        Invoke(nameof(EnableExitClick), 0.2f);
    }

    private void EnableExitClick()
    {
        canClickToExit = true;
    }
}