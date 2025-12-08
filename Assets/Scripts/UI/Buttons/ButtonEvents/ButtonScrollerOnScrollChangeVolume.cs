using UnityEngine;

public class ButtonScrollerOnScrollChangeVolume : MonoBehaviour
{
    const int MAX_VOLUME = 10;

    [SerializeField]
    private SoundManager.SoundTypes _volumeType;
    [SerializeField]
    private ButtonScroller _buttonScroller;

    private void Start()
    {
        _buttonScroller.OnScrollChanged += ButtonScroller_OnScrollChanged;
        switch (_volumeType)
        {
            case SoundManager.SoundTypes.MUSIC:
                _buttonScroller.CurrentValue = (int)((SoundManager.Instance?.SoundVolume.MusicVolume ?? 0.5f) * MAX_VOLUME);
                break;
            case SoundManager.SoundTypes.SFX:
                _buttonScroller.CurrentValue = (int)((SoundManager.Instance?.SoundVolume.SFXVolume ?? 0.5f) * MAX_VOLUME);
                break;
        }
    }

    private void ButtonScroller_OnScrollChanged(object sender, int volume)
    {
        if (SoundManager.Instance == null) return;

        switch (_volumeType)
        {
            case SoundManager.SoundTypes.MUSIC:
                SoundManager.Instance.SoundVolume.MusicVolume = (float)volume / MAX_VOLUME;
                break;
            case SoundManager.SoundTypes.SFX:
                SoundManager.Instance.SoundVolume.SFXVolume = (float)volume / MAX_VOLUME;
                break;
        }
        SoundManager.Instance.SaveSoundToJSON();
    }
}
