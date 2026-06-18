using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class ButtonScrollerOnScrollChangeVolume : MonoBehaviour
{
    const int MAX_VOLUME = 10;

    public enum ScrollerVolumeType
    {
        SFX,
        MUSIC
    }

    public ScrollerVolumeType VolumeType;
    [SerializeField]
    private ButtonScroller _buttonScroller;

    private void Start()
    {
        switch (VolumeType)
        {
            case ScrollerVolumeType.SFX:
                _buttonScroller.CurrentValue = (int)(SoundManager.Instance.SoundVolume.SFXVolume * MAX_VOLUME);
                break;
            case ScrollerVolumeType.MUSIC:
                _buttonScroller.CurrentValue = (int)(SoundManager.Instance.SoundVolume.MusicVolume * MAX_VOLUME);
                break;
        }

        _buttonScroller.OnScrollChanged += ButtonScroller_OnScrollChanged;
    }

    private void ButtonScroller_OnScrollChanged(object sender, int volume)
    {
        if (SoundManager.Instance == null) return;

        switch (VolumeType)
        {
            case ScrollerVolumeType.SFX:
                SoundManager.Instance.SoundVolume.SFXVolume = (float)volume / MAX_VOLUME;
                break;
            case ScrollerVolumeType.MUSIC:
                SoundManager.Instance.SoundVolume.MusicVolume = (float)volume / MAX_VOLUME;
                break;
        }

        SoundManager.Instance.SoundVolume.ApplyChanges();
        SoundManager.Instance.SaveSoundToJSON();
    }
}
