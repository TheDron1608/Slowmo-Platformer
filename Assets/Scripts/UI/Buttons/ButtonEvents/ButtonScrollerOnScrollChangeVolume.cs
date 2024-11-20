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
    }

    private void ButtonScroller_OnScrollChanged(object sender, int e)
    {
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
