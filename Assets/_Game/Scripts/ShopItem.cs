using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ShopItemType { Food, Medicine, Accessory }

public class ShopItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Config")]
    public string itemName = "Seed";
    public int price = 1;
    public ShopItemType itemType = ShopItemType.Food;

    [Header("Stat Configuration")]
    public int statRestoreAmount = 1;
    public float weightGainAmount = 0.5f;
    public AccessoryType accessoryType = AccessoryType.None;

    private Image shelfImage;
    private Canvas rootCanvas;
    private static GameObject activeFloatingProxy = null;

    void Awake()
    {
        shelfImage = GetComponent<Image>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeFloatingProxy == null)
        {
            bool isFreeStage = (GameManager.Instance.currentStage == LifeStage.Child || GameManager.Instance.currentStage == LifeStage.Elder);
            string text = isFreeStage ? itemName : $"{itemName}\n{price} C";
            TooltipManager.Instance.Show(text, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && activeFloatingProxy == null)
        {
            if (GameManager.Instance.TrySpendCoins(price))
            {
                SoundManager.Instance.PlayStandardClick();
                SpawnFloatingProxy();
            }
            else
            {
                SoundManager.Instance.PlayDenied();
                Debug.Log($"[ShopItem] Cannot afford {itemName} ({price} C).");
            }
        }
    }

    private void SpawnFloatingProxy()
    {
        TooltipManager.Instance.Hide();

        Color c = shelfImage.color;
        c.a = 0.3f;
        shelfImage.color = c;

        activeFloatingProxy = new GameObject("Floating_" + itemName, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        activeFloatingProxy.transform.SetParent(rootCanvas.transform, false);

        RectTransform rt = activeFloatingProxy.GetComponent<RectTransform>();
        RectTransform sourceRt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sourceRt.rect.width, sourceRt.rect.height);

        Image img = activeFloatingProxy.GetComponent<Image>();
        img.sprite = shelfImage.sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        CanvasGroup cg = activeFloatingProxy.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        FloatingProxyController controller = activeFloatingProxy.AddComponent<FloatingProxyController>();
        controller.Setup(this, rootCanvas);
    }

    public void OnAppliedSuccess()
    {
        if (GameManager.Instance.currentStage == LifeStage.Adult && GameManager.Instance.isRebelliousAdult)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.itemRefusalClip);
            GameManager.Instance.RegisterAdultRefusal();
            RestoreShelfIcon();
            return;
        }

        if (itemType == ShopItemType.Food)
        {
            bool wasAlreadyFull = GameManager.Instance.hunger >= 5;
            float effectiveWeight = wasAlreadyFull ? (weightGainAmount * 2.0f) : weightGainAmount;

            GameManager.Instance.ModifyHunger(statRestoreAmount);
            GameManager.Instance.ModifyWeight(effectiveWeight);
            SoundManager.Instance.PlaySFX(SoundManager.Instance.eatingClip);
        }
        else if (itemType == ShopItemType.Medicine)
        {
            GameManager.Instance.CureSicknessWithSyringe();
        }
        else if (itemType == ShopItemType.Accessory)
        {
            HamaCustomizationController.Instance.EquipAccessory(accessoryType);
            GameManager.Instance.ModifyHappiness(statRestoreAmount);
            SoundManager.Instance.PlaySFX(SoundManager.Instance.dressAccessoryClip);
        }

        RestoreShelfIcon();
    }

    public void OnCancelledRefund()
    {
        if (GameManager.Instance.currentStage == LifeStage.Adult)
        {
            GameManager.Instance.AddCoins(price);
        }
        RestoreShelfIcon();
    }

    private void RestoreShelfIcon()
    {
        Color c = shelfImage.color;
        c.a = 1f;
        shelfImage.color = c;
        activeFloatingProxy = null;
    }
}