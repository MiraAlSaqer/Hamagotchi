using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSoundHook : MonoBehaviour
{
    public enum SoundType { Standard, BackOrPageNav }

    [Header("Audio Settings")]
    [SerializeField] private SoundType soundType = SoundType.Standard;

    void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (soundType == SoundType.Standard)
        {
            SoundManager.Instance.PlayStandardClick();
        }
        else
        {
            SoundManager.Instance.PlayBackOrPage();
        }
    }
}