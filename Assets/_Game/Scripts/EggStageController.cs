using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EggStageController : MonoBehaviour
{
    public static EggStageController Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject topRow;
    [SerializeField] private GameObject bottomRow;
    [SerializeField] private GameObject clickToHatchPrompt;
    [SerializeField] private GameObject namingPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmNameButton;

    [Header("Animation Settings")]
    [SerializeField] private float hatchAnimationDuration = 1.5f;
    [SerializeField] private Animator hamaAnimator;
    [SerializeField] private HamaRoomController roomController;

    private static readonly int StageHash = Animator.StringToHash("StageState");
    private static readonly int HatchTriggerHash = Animator.StringToHash("Hatch");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private bool canClickHatch = false;
    private bool hasHatched = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        confirmNameButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    public void BeginEggStage()
    {
        topRow.SetActive(false);
        bottomRow.SetActive(false);
        clickToHatchPrompt.SetActive(false);
        namingPanel.SetActive(false);

        roomController.isRoaming = false;
        roomController.SetWalkingState(false);

        RectTransform rt = roomController.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, -18f);
        rt.localScale = Vector3.one;

        hamaAnimator.SetInteger(StageHash, 0);
        hamaAnimator.SetBool(IsWalkingHash, false);

        canClickHatch = false;
        hasHatched = false;
    }

    public void EnableHatching()
    {
        canClickHatch = true;
        clickToHatchPrompt.SetActive(true);
    }

    public void OnEggClicked()
    {
        if (!canClickHatch || hasHatched) return;
        StartCoroutine(HatchSequenceRoutine());
    }

    private IEnumerator HatchSequenceRoutine()
    {
        hasHatched = true;
        canClickHatch = false;
        clickToHatchPrompt.SetActive(false);

        SoundManager.Instance.PlaySFX(SoundManager.Instance.eggHatchClip);
        hamaAnimator.SetTrigger(HatchTriggerHash);

        yield return new WaitForSeconds(hatchAnimationDuration);

        namingPanel.SetActive(true);
        nameInputField.text = "";
        nameInputField.Select();
        nameInputField.ActivateInputField();
    }

    private void OnConfirmButtonClicked()
    {
        string chosenName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(chosenName))
        {
            chosenName = "HAMA";
        }

        GameManager.Instance.petName = chosenName;
        namingPanel.SetActive(false);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.ageUpChimeClip);
        CompleteEggStage();
    }

    private void CompleteEggStage()
    {
        topRow.SetActive(true);
        bottomRow.SetActive(true);
        GameManager.Instance.TransitionToStage(LifeStage.Child);
    }
}