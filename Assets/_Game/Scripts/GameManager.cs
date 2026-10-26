using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LifeStage { Egg, Child, Adult, Elder }
public enum WeightVisualState { Thin, Normal, Chubby }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State & Timers")]
    public bool acceleratedTestMode = true;
    public bool ignoreSaveFileForTesting = false;
    public string petName = "HAMA";
    public int ageInDays = 0;
    public LifeStage currentStage = LifeStage.Egg;
    [SerializeField] private float eggStageMinutes = 5.0f;

    [Header("Pet Core Stats")]
    public float currentWeight = 40.0f;
    [Range(0, 6)] public int hunger = 6;
    [Range(0, 6)] public int hygiene = 6;
    [Range(0, 6)] public int happiness = 6;
    [Range(0, 6)] public int energy = 6;
    public int currentCoins = 0;

    [Header("Status Flags & Penalties")]
    public bool isRebelliousAdult = false;
    public int adultRefusalCount = 0;
    public bool isSick = false;
    public bool isSleeping = false;
    [SerializeField] private float sicknessLethalDuration = 900f;
    [SerializeField] private float starvationGraceDuration = 300f;
    [SerializeField] private float boredomGraceDuration = 300f;
    [SerializeField] private float exhaustionGraceDuration = 300f;
    [SerializeField] private float poopCheckInterval = 540f;

    [Header("UI Elements & Prefabs")]
    [SerializeField] private Image sickOverlay;
    [SerializeField] private Image mainHamaDirtLayer;
    [SerializeField] private Button clinicButton;
    [SerializeField] private Button showerButton;
    [SerializeField] private Button sleepButton;
    [SerializeField] private Image sleepCurtain;
    [SerializeField] private GameObject poopPrefab;
    [SerializeField] private RectTransform mainRoomContainer;

    [Header("Thought Bubbles")]
    [SerializeField] private GameObject bubbleContainer;
    [SerializeField] private Image alertIcon;
    [SerializeField] private Image expressionIcon;
    [SerializeField] private Sprite hungerAlertSprite;
    [SerializeField] private Sprite dirtyAlertSprite;
    [SerializeField] private Sprite happinessAlertSprite;
    [SerializeField] private Sprite energyAlertSprite;
    [SerializeField] private Sprite[] randomMoodSprites;

    public int weightGrams => Mathf.RoundToInt(currentWeight);
    public int CurrentCoins => currentCoins;

    public event Action OnWeightChanged;
    public event Action<int> OnCoinsChanged;
    public event Action OnStatsChanged;

    public WeightVisualState CurrentWeightVisual
    {
        get
        {
            if (currentStage == LifeStage.Child)
                return currentWeight < 35.0f ? WeightVisualState.Thin : (currentWeight >= 45.0f ? WeightVisualState.Chubby : WeightVisualState.Normal);

            if (currentStage == LifeStage.Adult)
                return currentWeight < 75.0f ? WeightVisualState.Thin : (currentWeight >= 85.0f ? WeightVisualState.Chubby : WeightVisualState.Normal);

            return currentWeight < 95.0f ? WeightVisualState.Thin : (currentWeight >= 105.0f ? WeightVisualState.Chubby : WeightVisualState.Normal);
        }
    }

    private float eggTimer = 0f;
    private float sicknessTimer = 0f;
    private float poopTimer = 0f;
    private float starvationTimer = 0f;
    private float boredomTimer = 0f;
    private float exhaustionTimer = 0f;
    private float dayTimer = 0f;
    private float hungerTimer = 0f;
    private float funTimer = 0f;
    private float hygieneTimer = 0f;
    private float energyTimer = 0f;
    private float randomMoodTimer = 0f;
    private float nextMoodInterval = 25f;

    private int elderSicknessCheckedDay = -1;
    private bool bedtimeAvailable = false;
    private bool isDead = false;
    private Coroutine bubbleRoutine;
    private readonly List<GameObject> activePoops = new List<GameObject>();

    private const float BaseHungerTime = 300f;
    private const float BaseFunTime = 360f;
    private const float BaseHygieneTime = 480f;
    private const float BaseEnergyTime = 300f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        Application.runInBackground = true;

        if (ignoreSaveFileForTesting)
            Debug.LogWarning("Ignore save file");
        else
            LoadGame();
    }

    void Start()
    {
        HideBubbleInstant();
        sleepButton.interactable = bedtimeAvailable;

        if (currentStage == LifeStage.Egg)
        {
            EggStageController.Instance.BeginEggStage();
        }
        else
        {
            if (currentStage == LifeStage.Child) NavigationManager.Instance.ConfigureForChildStage();
            else if (currentStage == LifeStage.Adult) NavigationManager.Instance.ConfigureForAdultStage();
            else if (currentStage == LifeStage.Elder) NavigationManager.Instance.ConfigureForElderStage();

            UpdateClinicButtonState();
            UpdateHygieneVisuals();
            UpdateSicknessVisuals();
        }

        OnCoinsChanged?.Invoke(currentCoins);
        OnStatsChanged?.Invoke();
        OnWeightChanged?.Invoke();
    }

    void Update()
    {
        if (isSleeping || isDead) return;

        if (currentStage == LifeStage.Egg)
        {
            HideBubbleInstant();
            float incubationTarget = acceleratedTestMode ? 5.0f : (eggStageMinutes * 60f);

            if (eggTimer >= 0f)
            {
                eggTimer += Time.deltaTime;
                if (eggTimer >= incubationTarget)
                {
                    eggTimer = -1f;
                    EggStageController.Instance.EnableHatching();
                }
            }
            return;
        }

        float timeScale = acceleratedTestMode ? 60f : 1f;
        float stageMultiplier = (currentStage == LifeStage.Child || currentStage == LifeStage.Elder) ? 1.5f : 1.0f;
        float delta = Time.deltaTime * timeScale * stageMultiplier;

        float sleepUnlockTime = (currentStage == LifeStage.Child || currentStage == LifeStage.Elder) ? 1200f : 1800f;
        dayTimer += Time.deltaTime * timeScale;

        if (!bedtimeAvailable && dayTimer >= sleepUnlockTime)
        {
            bedtimeAvailable = true;
            sleepButton.interactable = true;
        }

        if (isSick)
        {
            sicknessTimer += delta;
            float maxSickTime = acceleratedTestMode ? (sicknessLethalDuration / 60f) : sicknessLethalDuration;
            if (sicknessTimer >= maxSickTime) TriggerDeath("Sickness");
        }
        else sicknessTimer = 0f;

        if (hunger <= 0)
        {
            starvationTimer += delta;
            float maxGrace = acceleratedTestMode ? (starvationGraceDuration / 60f) : starvationGraceDuration;
            if (starvationTimer >= maxGrace) TriggerDeath("Starvation");
        }
        else starvationTimer = 0f;

        if (happiness <= 0)
        {
            boredomTimer += delta;
            float maxBoredomGrace = acceleratedTestMode ? (boredomGraceDuration / 60f) : boredomGraceDuration;
            if (boredomTimer >= maxBoredomGrace) TriggerDeath("Boredom");
        }
        else boredomTimer = 0f;

        if (energy <= 0)
        {
            exhaustionTimer += delta;
            float maxExhaustionGrace = acceleratedTestMode ? (exhaustionGraceDuration / 60f) : exhaustionGraceDuration;
            if (exhaustionTimer >= maxExhaustionGrace) TriggerDeath("Exhaustion");
        }
        else exhaustionTimer = 0f;

        if (NavigationManager.Instance.IsInMainRoom)
        {
            poopTimer += delta;
            if (poopTimer >= poopCheckInterval)
            {
                poopTimer = 0f;
                if (UnityEngine.Random.value < 0.4f) SpawnPoop();
            }
        }

        if (hunger > 3 && hygiene > 3 && happiness > 3 && energy > 3 && !isSick)
        {
            randomMoodTimer += Time.deltaTime;
            float interval = acceleratedTestMode ? UnityEngine.Random.Range(4f, 8f) : nextMoodInterval;

            if (randomMoodTimer >= interval)
            {
                randomMoodTimer = 0f;
                nextMoodInterval = UnityEngine.Random.Range(20f, 40f);
                TriggerRandomMoodBubble();
            }
        }

        hungerTimer += delta;
        if (hungerTimer >= BaseHungerTime)
        {
            hungerTimer = 0f;
            ModifyHunger(-1);
            ModifyWeight(currentStage == LifeStage.Elder ? -0.25f : (currentStage == LifeStage.Child ? -0.1f : -0.2f));
            TryElderSickness();
        }

        funTimer += delta;
        if (funTimer >= BaseFunTime)
        {
            funTimer = 0f;
            ModifyHappiness(-1);
            TryElderSickness();
        }

        hygieneTimer += delta;
        if (hygieneTimer >= BaseHygieneTime)
        {
            hygieneTimer = 0f;
            ModifyHygiene(-1);
            TryElderSickness();
        }

        if (bedtimeAvailable)
        {
            energyTimer += delta;
            if (energyTimer >= BaseEnergyTime)
            {
                energyTimer = 0f;
                ModifyEnergy(-1);
                TryElderSickness();
            }
        }
    }

    private void LoadGame()
    {
        if (PlayerPrefs.GetInt("Save_Active", 0) == 1)
        {
            petName = PlayerPrefs.GetString("Save_PetName", "HAMA");
            currentStage = (LifeStage)PlayerPrefs.GetInt("Save_Stage");
            ageInDays = PlayerPrefs.GetInt("Save_Age");
            currentWeight = PlayerPrefs.GetFloat("Save_Weight");
            currentCoins = PlayerPrefs.GetInt("Save_Coins");
            hunger = PlayerPrefs.GetInt("Save_Hunger");
            hygiene = PlayerPrefs.GetInt("Save_Hygiene");
            happiness = PlayerPrefs.GetInt("Save_Happiness");
            energy = PlayerPrefs.GetInt("Save_Energy");
            isSick = PlayerPrefs.GetInt("Save_IsSick") == 1;
            isRebelliousAdult = PlayerPrefs.GetInt("Save_Rebellious") == 1;
            adultRefusalCount = PlayerPrefs.GetInt("Save_RefusalCount");
            bedtimeAvailable = PlayerPrefs.GetInt("Save_Bedtime") == 1;
            elderSicknessCheckedDay = PlayerPrefs.GetInt("Save_ElderSickDay", -1);

            eggTimer = PlayerPrefs.GetFloat("Save_EggT");
            dayTimer = PlayerPrefs.GetFloat("Save_DayT");
            sicknessTimer = PlayerPrefs.GetFloat("Save_SickT");
            starvationTimer = PlayerPrefs.GetFloat("Save_StarveT");
            poopTimer = PlayerPrefs.GetFloat("Save_PoopT");
            hungerTimer = PlayerPrefs.GetFloat("Save_HungerT");
            funTimer = PlayerPrefs.GetFloat("Save_FunT");
            hygieneTimer = PlayerPrefs.GetFloat("Save_HygieneT");
            energyTimer = PlayerPrefs.GetFloat("Save_EnergyT");
        }
        else
        {
            petName = "HAMA";
            currentStage = LifeStage.Egg;
            ageInDays = 0;
            currentWeight = 40.0f;
            currentCoins = 0;
            hunger = 6; hygiene = 6; happiness = 6; energy = 6;
            isSick = false; isRebelliousAdult = false; adultRefusalCount = 0;
            bedtimeAvailable = false;
            elderSicknessCheckedDay = -1;

            eggTimer = 0f; dayTimer = 0f; sicknessTimer = 0f; starvationTimer = 0f; boredomTimer = 0f; exhaustionTimer = 0f;
            poopTimer = 0f; hungerTimer = 0f; funTimer = 0f; hygieneTimer = 0f; energyTimer = 0f;

            PlayerPrefs.SetFloat("Save_EggT", 0f);
            PlayerPrefs.Save();
        }
    }

    private void SaveGame()
    {
        if (isDead || ignoreSaveFileForTesting) return;

        PlayerPrefs.SetInt("Save_Active", 1);
        PlayerPrefs.SetString("Save_PetName", petName);
        PlayerPrefs.SetInt("Save_Stage", (int)currentStage);
        PlayerPrefs.SetInt("Save_Age", ageInDays);
        PlayerPrefs.SetFloat("Save_Weight", currentWeight);
        PlayerPrefs.SetInt("Save_Coins", currentCoins);
        PlayerPrefs.SetInt("Save_Hunger", hunger);
        PlayerPrefs.SetInt("Save_Hygiene", hygiene);
        PlayerPrefs.SetInt("Save_Happiness", happiness);
        PlayerPrefs.SetInt("Save_Energy", energy);
        PlayerPrefs.SetInt("Save_IsSick", isSick ? 1 : 0);
        PlayerPrefs.SetInt("Save_Rebellious", isRebelliousAdult ? 1 : 0);
        PlayerPrefs.SetInt("Save_RefusalCount", adultRefusalCount);
        PlayerPrefs.SetInt("Save_Bedtime", bedtimeAvailable ? 1 : 0);
        PlayerPrefs.SetInt("Save_ElderSickDay", elderSicknessCheckedDay);

        PlayerPrefs.SetFloat("Save_EggT", eggTimer);
        PlayerPrefs.SetFloat("Save_DayT", dayTimer);
        PlayerPrefs.SetFloat("Save_SickT", sicknessTimer);
        PlayerPrefs.SetFloat("Save_StarveT", starvationTimer);
        PlayerPrefs.SetFloat("Save_PoopT", poopTimer);
        PlayerPrefs.SetFloat("Save_HungerT", hungerTimer);
        PlayerPrefs.SetFloat("Save_FunT", funTimer);
        PlayerPrefs.SetFloat("Save_HygieneT", hygieneTimer);
        PlayerPrefs.SetFloat("Save_EnergyT", energyTimer);

        PlayerPrefs.Save();
    }

    private void ClearSave()
    {
        if (ignoreSaveFileForTesting) return;
        PlayerPrefs.SetInt("Save_Active", 0);
        PlayerPrefs.SetFloat("Save_EggT", 0f);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit() => SaveGame();
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    [ContextMenu("DEBUG: Evolve to Child")]
    public void DebugForceChild() => TransitionToStage(LifeStage.Child);

    [ContextMenu("DEBUG: Evolve to Adult")]
    public void DebugForceAdult() => TransitionToStage(LifeStage.Adult);

    [ContextMenu("DEBUG: Evolve to Elder")]
    public void DebugForceElder() => TransitionToStage(LifeStage.Elder);

    private void TryElderSickness()
    {
        if (currentStage == LifeStage.Elder && !isSick && elderSicknessCheckedDay != ageInDays)
        {
            elderSicknessCheckedDay = ageInDays;
            if (UnityEngine.Random.value < 0.10f) SetSick(true);
        }
    }

    public void TransitionToStage(LifeStage newStage)
    {
        currentStage = newStage;
        HamaRoomController room = FindObjectOfType<HamaRoomController>();

        if (currentStage == LifeStage.Child)
        {
            ageInDays = 1;
            dayTimer = 0f;
            currentWeight = 40.0f;
            room.isRoaming = true;
            room.UpdateStageVisual();

            NavigationManager.Instance.ConfigureForChildStage();
        }
        else if (currentStage == LifeStage.Adult)
        {
            DisciplineController.Instance.HideAllMarks();
            DisciplineController.Instance.isMisbehaving = false;

            if (DisciplineController.Instance.IsUnderDisciplined())
                isRebelliousAdult = true;

            currentWeight = 80.0f;
            room.isRoaming = true;
            room.UpdateStageVisual();
            AddCoins(10);
            NavigationManager.Instance.ConfigureForAdultStage();
        }
        else if (currentStage == LifeStage.Elder)
        {
            DisciplineController.Instance.HideAllMarks();
            DisciplineController.Instance.isMisbehaving = false;

            currentWeight = 100.0f;
            room.isRoaming = true;
            room.UpdateStageVisual();
            NavigationManager.Instance.ConfigureForElderStage();
        }

        UpdateClinicButtonState();
        UpdateHygieneVisuals();
        UpdateSicknessVisuals();
        OnWeightChanged?.Invoke();
    }

    public void RegisterAdultRefusal()
    {
        adultRefusalCount++;
        TriggerBubbleAlert();
        if (adultRefusalCount >= 3) TriggerDeath("Self-Neglect");
    }

    public void TriggerRunawayEnding() => TriggerDeath("Runaway");

    public void SpawnPoop()
    {
        if (currentStage == LifeStage.Egg || !NavigationManager.Instance.IsInMainRoom) return;

        ModifyHygiene(-2);
        GameObject newPoop = Instantiate(poopPrefab, mainRoomContainer);
        activePoops.Add(newPoop);

        RectTransform rt = newPoop.GetComponent<RectTransform>();
        GameObject hama = GameObject.FindWithTag("Pet");

        float spawnX = hama.GetComponent<RectTransform>().anchoredPosition.x;
        float offsetX = UnityEngine.Random.Range(-10f, 10f);
        rt.anchoredPosition = new Vector2(spawnX + offsetX, -40f);

        SoundManager.Instance.PlaySFX(SoundManager.Instance.poopSpawnClip);
    }

    public void UnregisterPoop(GameObject poopObj)
    {
        if (activePoops.Contains(poopObj)) activePoops.Remove(poopObj);
    }

    public void SetPoopsVisible(bool visible)
    {
        activePoops.RemoveAll(item => item == null);
        for (int i = 0; i < activePoops.Count; i++)
        {
            activePoops[i].SetActive(visible);
        }
    }

    public void FeedPoopToHama()
    {
        hygiene = 0;
        SetSick(true);
        OnStatsChanged?.Invoke();
        UpdateClinicButtonState();
        UpdateHygieneVisuals();
        TriggerBubbleAlert();
    }

    public void SetSick(bool sick)
    {
        if (!isSick && sick)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.coughingSickClip);
        }

        isSick = sick;
        sicknessTimer = 0f;
        UpdateSicknessVisuals();
        UpdateClinicButtonState();
    }

    public void CureSicknessWithSyringe()
    {
        SetSick(false);
        ModifyHygiene(6);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.healSyringeClip);
    }

    private void UpdateSicknessVisuals()
    {
        sickOverlay.gameObject.SetActive(isSick);
    }

    public void ModifyWeight(float amount)
    {
        float minW = 25f; float maxW = 55f; float lethalW = 52f;

        if (currentStage == LifeStage.Adult) { minW = 60f; maxW = 95f; lethalW = 90f; }
        else if (currentStage == LifeStage.Elder) { minW = 85f; maxW = 125f; lethalW = 115f; }

        currentWeight = Mathf.Clamp(currentWeight + amount, minW, maxW);
        OnWeightChanged?.Invoke();

        if (currentWeight >= lethalW) TriggerDeath("Obesity");
    }

    public void TriggerDeath(string reason)
    {
        isSleeping = true;
        isDead = true;
        ClearSave();
        EndingManager.Instance.TriggerEnding(reason, petName, ageInDays);
    }

    public void ModifyHunger(int amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0, 6);
        OnStatsChanged?.Invoke();
        if (amount < 0 && hunger <= 3) TriggerBubbleAlert();
    }

    public void ModifyHygiene(int amount)
    {
        hygiene = Mathf.Clamp(hygiene + amount, 0, 6);
        OnStatsChanged?.Invoke();

        if (hygiene <= 0 && !isSick) SetSick(true);

        UpdateClinicButtonState();
        UpdateHygieneVisuals();
        if (amount < 0 && hygiene <= 3) TriggerBubbleAlert();
    }

    public void ModifyHappiness(int amount)
    {
        happiness = Mathf.Clamp(happiness + amount, 0, 6);
        OnStatsChanged?.Invoke();
        if (amount < 0 && happiness <= 3) TriggerBubbleAlert();
    }

    public void ModifyEnergy(int amount)
    {
        energy = Mathf.Clamp(energy + amount, 0, 6);
        OnStatsChanged?.Invoke();
        if (amount < 0 && energy <= 3) TriggerBubbleAlert();
    }

    public void UpdateClinicButtonState()
    {
        clinicButton.interactable = (hygiene <= 1 || isSick);
    }

    public void UpdateHygieneVisuals()
    {
        bool isDirty = hygiene <= 3;
        showerButton.interactable = isDirty;

        mainHamaDirtLayer.gameObject.SetActive(isDirty);
        if (isDirty)
        {
            Color c = mainHamaDirtLayer.color;
            c.a = (hygiene <= 1) ? 1.0f : 0.55f;
            mainHamaDirtLayer.color = c;
        }
    }

    public void TriggerBubbleAlert()
    {
        if (currentStage == LifeStage.Egg || !NavigationManager.Instance.IsInMainRoom) return;

        Sprite chosenSprite = null;
        if (isSick) chosenSprite = dirtyAlertSprite;
        else if (hunger <= 3) chosenSprite = hungerAlertSprite;
        else if (hygiene <= 3) chosenSprite = dirtyAlertSprite;
        else if (happiness <= 3) chosenSprite = happinessAlertSprite;
        else if (energy <= 3) chosenSprite = energyAlertSprite;

        if (chosenSprite == null) return;

        SoundManager.Instance.PlaySFX(SoundManager.Instance.alertSoundClip);

        if (bubbleRoutine != null) StopCoroutine(bubbleRoutine);
        bubbleRoutine = StartCoroutine(ShowAlertRoutine(chosenSprite, 5.0f));
    }

    private IEnumerator ShowAlertRoutine(Sprite icon, float duration)
    {
        expressionIcon.gameObject.SetActive(false);
        alertIcon.gameObject.SetActive(true);
        alertIcon.sprite = icon;

        bubbleContainer.SetActive(true);
        yield return new WaitForSeconds(duration);
        bubbleContainer.SetActive(false);
        alertIcon.gameObject.SetActive(false);
        bubbleRoutine = null;
    }

    public void TriggerRandomMoodBubble()
    {
        if (currentStage == LifeStage.Egg || !NavigationManager.Instance.IsInMainRoom) return;

        Sprite mood = randomMoodSprites[UnityEngine.Random.Range(0, randomMoodSprites.Length)];
        if (bubbleRoutine != null) StopCoroutine(bubbleRoutine);

        float duration = acceleratedTestMode ? 2.5f : 5.0f;
        bubbleRoutine = StartCoroutine(ShowExpressionRoutine(mood, duration));
    }

    private IEnumerator ShowExpressionRoutine(Sprite expressionSprite, float duration)
    {
        alertIcon.gameObject.SetActive(false);
        expressionIcon.gameObject.SetActive(true);
        expressionIcon.sprite = expressionSprite;

        bubbleContainer.SetActive(true);
        yield return new WaitForSeconds(duration);
        bubbleContainer.SetActive(false);
        expressionIcon.gameObject.SetActive(false);
        bubbleRoutine = null;
    }

    public void HideBubbleInstant()
    {
        if (bubbleRoutine != null) StopCoroutine(bubbleRoutine);
        bubbleContainer.SetActive(false);
        alertIcon.gameObject.SetActive(false);
        expressionIcon.gameObject.SetActive(false);
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public bool TrySpendCoins(int amount)
    {
        if (currentStage == LifeStage.Child || currentStage == LifeStage.Elder) return true;

        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            OnCoinsChanged?.Invoke(currentCoins);
            return true;
        }
        return false;
    }

    public void TriggerSleepRoutine(float durationSeconds = 5f)
    {
        if (isSleeping || !bedtimeAvailable) return;
        StartCoroutine(SleepRoutine(durationSeconds));
    }

    private IEnumerator SleepRoutine(float duration)
    {
        isSleeping = true;
        SetPoopsVisible(false);
        SoundManager.Instance.PlayLightSwitch();

        sleepCurtain.gameObject.SetActive(true);
        sleepCurtain.color = Color.black;

        yield return new WaitForSeconds(duration);

        ageInDays++;
        dayTimer = 0f;
        bedtimeAvailable = false;
        sleepButton.interactable = false;

        DisciplineController.Instance.OnNewDay();
        ModifyEnergy(6);

        sleepCurtain.gameObject.SetActive(false);
        SoundManager.Instance.PlayLightSwitch();

        if (NavigationManager.Instance.IsInMainRoom)
        {
            SetPoopsVisible(true);
        }

        isSleeping = false;

        if (currentStage == LifeStage.Child && ageInDays >= 3)
        {
            TransitionToStage(LifeStage.Adult);
            SoundManager.Instance.PlaySFX(SoundManager.Instance.ageUpChimeClip);
        }
        else if (currentStage == LifeStage.Adult && ageInDays >= 6)
        {
            TransitionToStage(LifeStage.Elder);
            SoundManager.Instance.PlaySFX(SoundManager.Instance.ageUpChimeClip);
        }
        else if (currentStage == LifeStage.Elder && ageInDays > 7)
        {
            TriggerDeath("Old Age");
        }
    }
}