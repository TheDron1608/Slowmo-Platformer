using System;
using UnityEngine;

public class ButtonScrollerOnScrollChangeVolume : MonoBehaviour
{
    enum VolumeType
    {
        MUSIC,
        SFX
    }

    [SerializeField]
    private VolumeType _volumeType;
    [SerializeField]
    private ButtonScroller _buttonScroller;

    private void Start()
    {
        _buttonScroller.OnScrollChanged += ButtonScroller_OnScrollChanged;
        switch (_volumeType)
        {
            case VolumeType.MUSIC:
                _buttonScroller.CurrentValue = SoundManager.Instance?.SoundVolume.MusicVolume ?? 5;
                break;
            case VolumeType.SFX:
                _buttonScroller.CurrentValue = SoundManager.Instance?.SoundVolume.SFXVolume ?? 5;
                break;
        }
    }

    private void ButtonScroller_OnScrollChanged(object sender, int e)
    {
        if (SoundManager.Instance == null) return;

        switch (_volumeType)
        {
            case VolumeType.MUSIC:
                SoundManager.Instance.SoundVolume.MusicVolume = e;
                break;
            case VolumeType.SFX:
                SoundManager.Instance.SoundVolume.SFXVolume = e;
                break;
        }
        SoundManager.Instance.SaveSoundToJSON();
    }
}
