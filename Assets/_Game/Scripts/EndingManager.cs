using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject endingCanvas;
    [SerializeField] private Image fadeImage;
    [SerializeField] private TextMeshProUGUI topDeathTitleText;
    [SerializeField] private TextMeshProUGUI bottomDetailsText;
    [SerializeField] private TextMeshProUGUI collectionCounterText;
    [SerializeField] private GameObject downloadButtonObj;
    [SerializeField] private Image endingPanelBackground;

    [Header("Screen Settings")]
    [SerializeField] private int endingWidth = 700;
    [SerializeField] private int endingHeight = 440;

    [Header("Ending Sprites")]
    [SerializeField] private Sprite starvationSprite;
    [SerializeField] private Sprite exhaustionSprite;
    [SerializeField] private Sprite boredomSprite;
    [SerializeField] private Sprite freezingSprite;
    [SerializeField] private Sprite burningSprite;
    [SerializeField] private Sprite sicknessSprite;
    [SerializeField] private Sprite obesitySprite;
    [SerializeField] private Sprite neglectSprite;
    [SerializeField] private Sprite runawaySprite;
    [SerializeField] private Sprite oldAgeSprite;

    [Header("Ending Audio")]
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip fliesSound;
    [SerializeField] private AudioClip nightSound;
    [SerializeField] private AudioClip happySound;
    [SerializeField] private AudioClip coldSound;
    [SerializeField] private AudioClip windSound;

    private readonly string[] allDeaths = {
        "Starvation", "Exhaustion", "Boredom", "Freezing", "Burning",
        "Sickness", "Obesity", "Self-Neglect", "Runaway", "Old Age"
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        endingCanvas.SetActive(false);
    }

    public void TriggerEnding(string deathType, string petName, int petAge)
    {
        StartCoroutine(EndingRoutine(deathType, petName, petAge));
    }

    private IEnumerator EndingRoutine(string rawDeathType, string petName, int petAge)
    {
        string deathType = rawDeathType.Split('(')[0].Trim();

        endingCanvas.SetActive(true);
        endingPanelBackground.gameObject.SetActive(false);

        fadeImage.gameObject.SetActive(true);
        Color fadeColor = fadeImage.color;
        fadeColor.a = 0f;
        fadeImage.color = fadeColor;

        float fadeDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }

        DesktopWindowManager.Instance.ResizeWindow(endingWidth, endingHeight, false);

        SetupEndingVisuals(deathType, petName, petAge);
        UpdateDeathCollection(deathType);

        endingPanelBackground.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }

    private void SetupEndingVisuals(string deathType, string petName, int petAge)
    {
        topDeathTitleText.text = $"DEATH BY {deathType.ToUpper()}";

        string causeDesc = "";
        AudioClip soundToPlay = null;

        if (deathType == "Starvation") { causeDesc = "Remained hungry for too long"; endingPanelBackground.sprite = starvationSprite; soundToPlay = emptySound; }
        else if (deathType == "Exhaustion") { causeDesc = "Exhaustion meter fully depleted"; endingPanelBackground.sprite = exhaustionSprite; soundToPlay = nightSound; }
        else if (deathType == "Boredom") { causeDesc = "Died from doing nothing"; endingPanelBackground.sprite = boredomSprite; soundToPlay = emptySound; }
        else if (deathType == "Freezing") { causeDesc = "Shower was too cold"; endingPanelBackground.sprite = freezingSprite; soundToPlay = coldSound; }
        else if (deathType == "Burning") { causeDesc = "Exposed to fatal heat levels"; endingPanelBackground.sprite = burningSprite; soundToPlay = fireSound; }
        else if (deathType == "Sickness") { causeDesc = "Illness was left untreated"; endingPanelBackground.sprite = sicknessSprite; soundToPlay = fliesSound; }
        else if (deathType == "Obesity") { causeDesc = "Hamster got too fat to function"; endingPanelBackground.sprite = obesitySprite; soundToPlay = fliesSound; }
        else if (deathType == "Self-Neglect") { causeDesc = "Didn't listen due to lack of discipline"; endingPanelBackground.sprite = neglectSprite; soundToPlay = windSound; }
        else if (deathType == "Runaway") { causeDesc = "Fled due to getting punished a lot"; endingPanelBackground.sprite = runawaySprite; soundToPlay = windSound; }
        else if (deathType == "Old Age") { causeDesc = "Lived a long and fulfilled life"; endingPanelBackground.sprite = oldAgeSprite; soundToPlay = happySound; }
        else { causeDesc = "An unknown fate."; }

        bottomDetailsText.text = $"NAME: {petName}\nAGE: {petAge} DAYS OLD\nCAUSE: {causeDesc}";
        SoundManager.Instance.PlaySFX(soundToPlay);
    }

    private void UpdateDeathCollection(string currentDeath)
    {
        PlayerPrefs.SetInt("Death_" + currentDeath, 1);
        PlayerPrefs.Save();

        int unlockedCount = 0;
        foreach (string death in allDeaths)
        {
            if (PlayerPrefs.GetInt("Death_" + death, 0) == 1)
            {
                unlockedCount++;
            }
        }

        collectionCounterText.text = $"{unlockedCount}/10\nDEATHS\nUNLOCKED";
    }

    public void OnDownloadImageClicked()
    {
        StartCoroutine(CaptureScreenshotRoutine());
    }

    private IEnumerator CaptureScreenshotRoutine()
    {
        downloadButtonObj.SetActive(false);
        collectionCounterText.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"Hamagotchi_{timestamp}.png";

        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string fullPath = Path.Combine(desktopPath, filename);

        ScreenCapture.CaptureScreenshot(fullPath);
        UnityEngine.Debug.Log($"Screenshot saved to: {fullPath}");

        downloadButtonObj.SetActive(true);
        collectionCounterText.gameObject.SetActive(true);
    }
}