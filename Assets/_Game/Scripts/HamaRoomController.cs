using UnityEngine;
using UnityEngine.EventSystems;

public class HamaRoomController : MonoBehaviour, IPointerClickHandler
{
    [Header("Components & Bounds")]
    [SerializeField] private Animator animator;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float minX = -45f;
    [SerializeField] private float maxX = 45f;
    [SerializeField] private float walkSpeed = 20f;

    public bool isRoaming = true;
    public bool isShowerWalking = false;

    private int direction = 1;
    private float stateTimer = 0f;
    private bool isWalking = false;

    private static readonly int WeightStateHash = Animator.StringToHash("WeightState");
    private static readonly int WeightFloatHash = Animator.StringToHash("WeightFloat");
    private static readonly int StageHash = Animator.StringToHash("StageState");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private static readonly int ChildLocoHash = Animator.StringToHash("ChildLocomotion");
    private static readonly int AdultLocoHash = Animator.StringToHash("AdultLocomotion");
    private static readonly int ElderLocoHash = Animator.StringToHash("ElderLocomotion");

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        GameManager.Instance.OnWeightChanged += UpdateWeightVisual;
        UpdateWeightVisual();
        UpdateStageVisual();
    }

    void OnDisable()
    {
        GameManager.Instance.OnWeightChanged -= UpdateWeightVisual;
    }

    void Start()
    {
        UpdateWeightVisual();
        UpdateStageVisual();

        rectTransform.anchoredPosition = new Vector2(23f, rectTransform.anchoredPosition.y);

        if (GameManager.Instance.currentStage == LifeStage.Child)
        {
            animator.Play("Child_BlendTree", 0, 0f);
        }
        else if (GameManager.Instance.currentStage == LifeStage.Adult)
        {
            animator.Play("Adult_BlendTree", 0, 0f);
        }
        else if (GameManager.Instance.currentStage == LifeStage.Elder)
        {
            animator.Play("Elder_BlendTree", 0, 0f);
        }
        else if (GameManager.Instance.currentStage == LifeStage.Egg)
        {
            animator.Play("Idle_Egg", 0, 0f);
            LockInEggPosition();
            return;
        }

        PickNewMainRoomState();
    }

    public void LockInEggPosition()
    {
        isRoaming = false;
        isWalking = false;

        rectTransform.anchoredPosition = new Vector2(23f, -18f);
        rectTransform.localScale = Vector3.one;

        SetWalkingState(false);
    }

    public void UpdateWeightVisual()
    {
        int weightValue = (int)GameManager.Instance.CurrentWeightVisual;
        animator.SetInteger(WeightStateHash, weightValue);
        animator.SetFloat(WeightFloatHash, (float)weightValue);
    }

    public void UpdateStageVisual()
    {
        animator.SetInteger(StageHash, (int)GameManager.Instance.currentStage);
    }

    void Update()
    {
        if (GameManager.Instance.currentStage == LifeStage.Egg)
        {
            if (isRoaming || isWalking)
            {
                LockInEggPosition();
            }
            return;
        }

        if (isShowerWalking)
        {
            MovePacing(maxX, minX, walkSpeed * 1.2f);
            SetWalkingState(true);
            return;
        }

        if (isRoaming)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewMainRoomState();
            }

            if (isWalking)
            {
                MovePacing(maxX, minX, walkSpeed);
            }

            SetWalkingState(isWalking);
        }
        else
        {
            isWalking = false;
            SetWalkingState(false);
        }
    }

    private void MovePacing(float rightBound, float leftBound, float speed)
    {
        float newX = rectTransform.anchoredPosition.x + (direction * speed * Time.deltaTime);

        if (newX >= rightBound)
        {
            newX = rightBound;
            direction = -1;
            FaceDirection(direction);
        }
        else if (newX <= leftBound)
        {
            newX = leftBound;
            direction = 1;
            FaceDirection(direction);
        }

        rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);
    }

    private void FaceDirection(int dir)
    {
        rectTransform.localScale = new Vector3(dir > 0 ? 1f : -1f, 1f, 1f);
    }

    private void PickNewMainRoomState()
    {
        isWalking = Random.value > 0.5f;
        stateTimer = Random.Range(2.5f, 5.0f);

        if (isWalking)
        {
            direction = Random.value > 0.5f ? 1 : -1;
            FaceDirection(direction);
        }
    }

    public void SetWalkingState(bool walking)
    {
        isWalking = walking;
        float speedVal = walking ? 1.0f : 0.0f;

        if (GameManager.Instance.currentStage == LifeStage.Child)
        {
            animator.SetFloat(ChildLocoHash, speedVal);
        }
        else if (GameManager.Instance.currentStage == LifeStage.Elder)
        {
            animator.SetFloat(ElderLocoHash, speedVal);
        }
        else
        {
            animator.SetFloat(AdultLocoHash, speedVal);
        }

        animator.SetBool(IsWalkingHash, walking);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.currentStage == LifeStage.Egg)
        {
            EggStageController.Instance.OnEggClicked();
        }
    }
}