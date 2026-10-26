using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance { get; private set; }

    [Header("Persistent Main Elements")]
    [SerializeField] private GameObject mainRoom;
    [SerializeField] private GameObject bg;
    [SerializeField] private RectTransform hamaRect;
    [SerializeField] private float hamaDefaultX = 0f;
    [SerializeField] private float hamaRibbonShiftX = -35f;
    [SerializeField] private float hamaMoveDuration = 0.25f;

    [Header("Menu UI & Animations")]
    [SerializeField] private RectTransform topRow;
    [SerializeField] private RectTransform bottomRow;
    [SerializeField] private float bannerSlideDuration = 0.25f;
    [SerializeField] private float topRowOffscreenY = 160f;
    [SerializeField] private float bottomRowOffscreenY = -160f;
    [SerializeField] private GameObject coinDisplay;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Stage Action Buttons")]
    [SerializeField] private Button wardrobeButton;
    [SerializeField] private Button disciplineButton;
    [SerializeField] private Button runnerButton;

    [Header("Sub-Room Panels")]
    [SerializeField] private GameObject subRoomsContainer;
    [SerializeField] private GameObject panelShower;
    [SerializeField] private GameObject panelFood;
    [SerializeField] private GameObject panelWardrobe;
    [SerializeField] private GameObject panelClinic;
    [SerializeField] private GameObject panelRunnerGame;
    [SerializeField] private GameObject panelPetInfo;

    private float topRowOriginalY;
    private float bottomRowOriginalY;
    private Tween topTween;
    private Tween bottomTween;
    private Tween hamaTween;

    public bool IsInMainRoom => (mainRoom.activeSelf && !subRoomsContainer.activeSelf);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        topRowOriginalY = topRow.anchoredPosition.y;
        bottomRowOriginalY = bottomRow.anchoredPosition.y;
    }

    void Start()
    {
        GameManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
        UpdateCoinDisplay(GameManager.Instance.CurrentCoins);
        SyncStageLocks(GameManager.Instance.currentStage);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
        topTween?.Kill();
        bottomTween?.Kill();
        hamaTween?.Kill();
    }

    public void SyncStageLocks(LifeStage stage)
    {
        if (stage == LifeStage.Child) ConfigureForChildStage();
        else if (stage == LifeStage.Adult) ConfigureForAdultStage();
        else if (stage == LifeStage.Elder) ConfigureForElderStage();
        else if (stage == LifeStage.Egg)
        {
            wardrobeButton.interactable = false;
            disciplineButton.interactable = false;
            runnerButton.interactable = false;
            coinDisplay.SetActive(false);
        }
    }

    public void UpdateCoinDisplay(int coins) => coinText.text = coins.ToString();

    public void ConfigureForChildStage()
    {
        wardrobeButton.interactable = false;
        disciplineButton.interactable = true;
        runnerButton.interactable = true;
        coinDisplay.SetActive(false);
    }

    public void ConfigureForAdultStage()
    {
        wardrobeButton.interactable = true;
        disciplineButton.interactable = false;
        runnerButton.interactable = true;
    }

    public void ConfigureForElderStage()
    {
        wardrobeButton.interactable = false;
        disciplineButton.interactable = false;
        runnerButton.interactable = false;
        coinDisplay.SetActive(false);
    }

    public void ReturnToMainRoom() => ReturnToMainRoom(false);

    public void ReturnToMainRoom(bool instant)
    {
        bool isEgg = GameManager.Instance.currentStage == LifeStage.Egg;

        mainRoom.SetActive(true);
        bg.SetActive(true);
        hamaRect.gameObject.SetActive(true);
        hamaTween?.Kill();

        if (instant || isEgg)
        {
            hamaRect.anchoredPosition = new Vector2(hamaDefaultX, hamaRect.anchoredPosition.y);
        }
        else
        {
            hamaTween = hamaRect.DOAnchorPosX(hamaDefaultX, hamaMoveDuration).SetEase(Ease.OutQuad);
        }

        HideAllSubPanels();
        subRoomsContainer.SetActive(false);
        coinDisplay.SetActive(false);

        if (!isEgg)
        {
            if (instant)
            {
                topTween?.Kill();
                bottomTween?.Kill();
                topRow.anchoredPosition = new Vector2(topRow.anchoredPosition.x, topRowOriginalY);
                bottomRow.anchoredPosition = new Vector2(bottomRow.anchoredPosition.x, bottomRowOriginalY);
            }
            else
            {
                AnimateBanners(show: true);
            }
            GameManager.Instance.TriggerBubbleAlert();
        }

        var controller = hamaRect.GetComponent<HamaRoomController>();
        controller.isRoaming = !isEgg;
        controller.isShowerWalking = false;

        if (!isEgg)
        {
            GameManager.Instance.SetPoopsVisible(true);
        }
    }

    private void HideAllSubPanels()
    {
        panelShower.SetActive(false);
        panelFood.SetActive(false);
        panelWardrobe.SetActive(false);
        panelClinic.SetActive(false);
        panelRunnerGame.SetActive(false);
        panelPetInfo.SetActive(false);
    }

    public void OpenFood() => SetupRibbonRoom(panelFood);

    public void OpenWardrobe()
    {
        if (GameManager.Instance.currentStage == LifeStage.Child || GameManager.Instance.currentStage == LifeStage.Elder) return;
        SetupRibbonRoom(panelWardrobe);
    }

    public void OpenClinic() => SetupRibbonRoom(panelClinic);

    private void SetupRibbonRoom(GameObject panel)
    {
        AnimateBanners(show: false);
        OpenPanelContainer(panel);

        bg.SetActive(true);
        hamaRect.gameObject.SetActive(true);

        hamaTween?.Kill();
        hamaTween = hamaRect.DOAnchorPosX(hamaRibbonShiftX, hamaMoveDuration).SetEase(Ease.OutQuad);

        bool showCoins = GameManager.Instance.currentStage == LifeStage.Adult;
        coinDisplay.SetActive(showCoins);

        var controller = hamaRect.GetComponent<HamaRoomController>();
        controller.isRoaming = false;
        controller.isShowerWalking = false;
    }

    public void OpenPetInfo()
    {
        AnimateBanners(show: false);
        OpenPanelContainer(panelPetInfo);

        bg.SetActive(true);
        hamaRect.gameObject.SetActive(false);
        coinDisplay.SetActive(false);
    }

    public void OpenShower()
    {
        AnimateBanners(show: false);
        OpenPanelContainer(panelShower);

        bg.SetActive(true);
        hamaRect.gameObject.SetActive(false);
        coinDisplay.SetActive(false);
    }

    public void ShowHamaForShowerStage2()
    {
        hamaRect.anchoredPosition = new Vector2(hamaDefaultX, hamaRect.anchoredPosition.y);
        hamaRect.gameObject.SetActive(true);

        var controller = hamaRect.GetComponent<HamaRoomController>();
        controller.isRoaming = false;
        controller.isShowerWalking = true;
    }

    public void OpenRunner()
    {
        if (GameManager.Instance.currentStage == LifeStage.Elder) return;

        if (GameManager.Instance.energy <= 0 || GameManager.Instance.isSleeping)
        {
            GameManager.Instance.TriggerBubbleAlert();
            return;
        }

        AnimateBanners(show: false);
        OpenPanelContainer(panelRunnerGame);

        bg.SetActive(false);
        hamaRect.gameObject.SetActive(false);
        coinDisplay.SetActive(false);
    }

    private void OpenPanelContainer(GameObject panel)
    {
        subRoomsContainer.SetActive(true);
        HideAllSubPanels();
        panel.SetActive(true);
        GameManager.Instance.SetPoopsVisible(false);
    }

    private void AnimateBanners(bool show)
    {
        topTween?.Kill();
        bottomTween?.Kill();

        float targetTopY = show ? topRowOriginalY : topRowOffscreenY;
        float targetBottomY = show ? bottomRowOriginalY : bottomRowOffscreenY;

        topTween = topRow.DOAnchorPosY(targetTopY, bannerSlideDuration).SetEase(Ease.OutQuad);
        bottomTween = bottomRow.DOAnchorPosY(targetBottomY, bannerSlideDuration).SetEase(Ease.OutQuad);
    }
}