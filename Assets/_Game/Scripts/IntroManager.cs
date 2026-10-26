using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class IntroManager : MonoBehaviour
{
    [Header("UI & Cinematics")]
    [SerializeField] private GameObject introCanvas;
    [SerializeField] private GameObject mainMenuManagerObject;
    [SerializeField] private RawImage introPanelRawImage;
    [SerializeField] private Texture[] storyTextures;
    [SerializeField] private PlayableDirector introTimeline;
    [SerializeField] private Image fadeImage;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;

    private int currentPage = 0;
    private bool isTransitioning = false;

    void Awake()
    {
        Time.timeScale = 0f;
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("HasSeenIntro", 0) == 1)
        {
            DesktopWindowManager.Instance.ResizeWindow(280, 252, true);
            introCanvas.SetActive(false);
            mainMenuManagerObject.SetActive(true);
            this.enabled = false;
            return;
        }

        DesktopWindowManager.Instance.ResizeWindow(700, 440, false);

        if (storyTextures.Length > 0)
        {
            introPanelRawImage.texture = storyTextures[0];
        }

        fadeImage.gameObject.SetActive(false);
        introTimeline.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Input.GetMouseButtonDown(0))
        {
            AdvanceSequence();
        }
    }

    private void AdvanceSequence()
    {
        SoundManager.Instance.PlaySFX(clickSound);

        if (currentPage == storyTextures.Length - 1)
        {
            StartCoroutine(PlayTimelineAndTransition());
            return;
        }

        currentPage++;
        introPanelRawImage.texture = storyTextures[currentPage];
    }

    private IEnumerator PlayTimelineAndTransition()
    {
        isTransitioning = true;
        PlayerPrefs.SetInt("HasSeenIntro", 1);
        PlayerPrefs.Save();

        introTimeline.Play();
        yield return new WaitForSecondsRealtime((float)introTimeline.duration);

        fadeImage.gameObject.SetActive(true);
        Color fadeColor = fadeImage.color;
        fadeColor.a = 0f;
        float elapsed = 0f;
        float fadeDuration = 1.0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeColor.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }

        DesktopWindowManager.Instance.ResizeWindow(280, 252, true);

        yield return new WaitForSecondsRealtime(0.5f);

        introCanvas.SetActive(false);
        mainMenuManagerObject.SetActive(true);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }
}