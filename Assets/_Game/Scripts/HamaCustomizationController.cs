using UnityEngine;

public enum AccessoryType
{
    None,
    Tie,
    Sunglasses,
    Bow,
    Crown
}

public class HamaCustomizationController : MonoBehaviour
{
    public static HamaCustomizationController Instance { get; private set; }

    [System.Serializable]
    public struct AccessoryVisualPair
    {
        public AccessoryType type;
        public GameObject idleObject;
        public GameObject walkObject;
    }

    [Header("Accessories & Animation")]
    [SerializeField] private AccessoryVisualPair[] accessories;
    [SerializeField] private Animator hamaAnimator;

    private AccessoryType currentAccessory = AccessoryType.None;
    private bool isWalking = false;
    private int isWalkingHash;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        isWalkingHash = Animator.StringToHash("IsWalking");
    }

    void Start()
    {
        UpdateVisuals(true);
    }

    void Update()
    {
        bool animatorIsWalking = hamaAnimator.GetBool(isWalkingHash);
        SetWalking(animatorIsWalking);
    }

    public void SetWalking(bool walking)
    {
        if (isWalking == walking) return;

        isWalking = walking;
        UpdateVisuals(false);
    }

    public void EquipAccessory(AccessoryType type)
    {
        if (currentAccessory == type)
        {
            currentAccessory = AccessoryType.None;
        }
        else
        {
            currentAccessory = type;
        }

        UpdateVisuals(true);
    }

    public void UnequipCurrent()
    {
        currentAccessory = AccessoryType.None;
        UpdateVisuals(true);
    }

    private void UpdateVisuals(bool force)
    {
        for (int i = 0; i < accessories.Length; i++)
        {
            bool isCurrent = (accessories[i].type == currentAccessory);
            bool shouldIdleBeActive = isCurrent && !isWalking;
            bool shouldWalkBeActive = isCurrent && isWalking;

            if (force || accessories[i].idleObject.activeSelf != shouldIdleBeActive)
            {
                accessories[i].idleObject.SetActive(shouldIdleBeActive);
            }

            if (force || accessories[i].walkObject.activeSelf != shouldWalkBeActive)
            {
                accessories[i].walkObject.SetActive(shouldWalkBeActive);
            }
        }
    }
}