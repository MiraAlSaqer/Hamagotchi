using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("UI & Navigation")]
    public AudioClip standardButtonClickClip;
    public AudioClip backOrPageButtonClip;
    public AudioClip lightSwitchClip;
    public AudioClip insufficientFundsDeniedClip;

    [Header("Pet & Environment")]
    public AudioClip alertSoundClip;
    public AudioClip poopSpawnClip;
    public AudioClip trashDumpClip;
    public AudioClip coughingSickClip;

    [Header("Items & Interactions")]
    public AudioClip eatingClip;
    public AudioClip dressAccessoryClip;
    public AudioClip healSyringeClip;
    public AudioClip itemRefusalClip;

    [Header("Minigames")]
    public AudioClip showerWaterLoopClip;
    public AudioClip showerSuccessClip;
    public AudioClip wrongTemperatureClip;
    public AudioClip runnerJumpClip;
    public AudioClip runnerGameOverClip;

    [Header("Life Cycle")]
    public AudioClip eggHatchClip;
    public AudioClip ageUpChimeClip;
    public AudioClip whipDisciplineClip;
    public AudioClip childMisbehavingClip;
    public AudioClip misbehaviorPopClip;

    private AudioSource[] sfxSources;
    private AudioSource showerLoopSource;
    private const int PoolSize = 10;
    private int currentSourceIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        SetupInternalAudioPool();
    }

    private void SetupInternalAudioPool()
    {
        sfxSources = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFX_Source_{i}");
            sfxObj.transform.SetParent(transform);

            AudioSource src = sfxObj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            sfxSources[i] = src;
        }

        GameObject showerObj = new GameObject("Shower_Loop_Source");
        showerObj.transform.SetParent(transform);

        showerLoopSource = showerObj.AddComponent<AudioSource>();
        showerLoopSource.playOnAwake = false;
        showerLoopSource.loop = true;
        showerLoopSource.spatialBlend = 0f;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = sfxSources[currentSourceIndex];
        source.pitch = 1f;
        source.volume = volume;
        source.PlayOneShot(clip);

        currentSourceIndex = (currentSourceIndex + 1) % PoolSize;
    }

    public void PlayStandardClick() => PlaySFX(standardButtonClickClip);
    public void PlayBackOrPage() => PlaySFX(backOrPageButtonClip);
    public void PlayLightSwitch() => PlaySFX(lightSwitchClip);
    public void PlayDenied() => PlaySFX(insufficientFundsDeniedClip);

    public void StartShowerWater()
    {
        if (!showerLoopSource.isPlaying)
        {
            showerLoopSource.clip = showerWaterLoopClip;
            showerLoopSource.Play();
        }
    }

    public void PauseShowerWater()
    {
        if (showerLoopSource.isPlaying)
        {
            showerLoopSource.Pause();
        }
    }

    public void StopShowerWater()
    {
        showerLoopSource.Stop();
    }
}