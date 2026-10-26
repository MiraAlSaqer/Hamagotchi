using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private Image idlePetImage;
    [SerializeField] private GameObject hamaGameplayObject;

    [Header("Idle Sprites")]
    [SerializeField] private Sprite eggIdleSprite;
    [SerializeField] private Sprite childIdleSprite;
    [SerializeField] private Sprite adultIdleSprite;
    [SerializeField] private Sprite elderIdleSprite;

    void Awake()
    {
        Time.timeScale = 0f;
        mainMenuCanvas.SetActive(true);
        hamaGameplayObject.SetActive(false);
    }

    void Start()
    {
        UpdateIdleSprite();
    }

    private void UpdateIdleSprite()
    {
        LifeStage current = GameManager.Instance.currentStage;

        if (current == LifeStage.Egg) idlePetImage.sprite = eggIdleSprite;
        else if (current == LifeStage.Child) idlePetImage.sprite = childIdleSprite;
        else if (current == LifeStage.Adult) idlePetImage.sprite = adultIdleSprite;
        else if (current == LifeStage.Elder) idlePetImage.sprite = elderIdleSprite;
    }

    public void OnStartClicked()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.alertSoundClip);

        Time.timeScale = 1f;
        mainMenuCanvas.SetActive(false);
        hamaGameplayObject.SetActive(true);

        NavigationManager.Instance.ReturnToMainRoom(true);

        if (GameManager.Instance.currentStage == LifeStage.Egg)
        {
            EggStageController.Instance.BeginEggStage();

            if (PlayerPrefs.GetFloat("Save_EggT", 0f) == -1f)
            {
                EggStageController.Instance.EnableHatching();
            }
        }
        else
        {
            if (GameManager.Instance.currentStage == LifeStage.Child) NavigationManager.Instance.ConfigureForChildStage();
            else if (GameManager.Instance.currentStage == LifeStage.Adult) NavigationManager.Instance.ConfigureForAdultStage();
            else if (GameManager.Instance.currentStage == LifeStage.Elder) NavigationManager.Instance.ConfigureForElderStage();

            GameManager.Instance.UpdateClinicButtonState();
            GameManager.Instance.UpdateHygieneVisuals();
        }
    }
}