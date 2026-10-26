using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoMenuUI : MonoBehaviour
{
    [Header("Pages & Labels")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI weightText;

    [Header("Meters & Toggles")]
    [SerializeField] private Image[] hungerPips = new Image[6];
    [SerializeField] private Image[] hygienePips = new Image[6];
    [SerializeField] private Image[] happinessPips = new Image[6];
    [SerializeField] private Image[] energyPips = new Image[6];
    [SerializeField] private GameObject happinessTextObj;
    [SerializeField] private GameObject happinessPipsObj;

    private const float EmptyAlpha = 100f / 255f;
    private const float FullAlpha = 1f;

    void OnEnable()
    {
        ShowPage1();
        RefreshAll();

        GameManager.Instance.OnStatsChanged += RefreshAll;
        GameManager.Instance.OnWeightChanged += RefreshPage1;
    }

    void OnDisable()
    {
        GameManager.Instance.OnStatsChanged -= RefreshAll;
        GameManager.Instance.OnWeightChanged -= RefreshPage1;
    }

    public void ShowPage1()
    {
        page1.SetActive(true);
        page2.SetActive(false);
        RefreshPage1();
    }

    public void ShowPage2()
    {
        page1.SetActive(false);
        page2.SetActive(true);
        RefreshMeters();
    }

    public void RefreshAll()
    {
        RefreshPage1();
        RefreshMeters();
    }

    private void RefreshPage1()
    {
        nameText.text = $"NAME: {GameManager.Instance.petName}";
        ageText.text = $"AGE: {GameManager.Instance.ageInDays} DAYS";
        weightText.text = $"WEIGHT: {GameManager.Instance.weightGrams}G";
    }

    private void RefreshMeters()
    {
        bool showHappiness = GameManager.Instance.currentStage != LifeStage.Elder;
        happinessTextObj.SetActive(showHappiness);
        happinessPipsObj.SetActive(showHappiness);

        UpdateRow(hungerPips, GameManager.Instance.hunger);
        UpdateRow(hygienePips, GameManager.Instance.hygiene);

        if (showHappiness)
        {
            UpdateRow(happinessPips, GameManager.Instance.happiness);
        }

        UpdateRow(energyPips, GameManager.Instance.energy);
    }

    private void UpdateRow(Image[] pips, int activeCount)
    {
        for (int i = 0; i < pips.Length; i++)
        {
            Color c = pips[i].color;
            c.a = (i < activeCount) ? FullAlpha : EmptyAlpha;
            pips[i].color = c;
        }
    }
}