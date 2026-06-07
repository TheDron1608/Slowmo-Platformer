using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum SoundTypes
    {
        SFX,
        MUSIC
    }

    [Serializable]
    public class SoundData
    {
        private float _musicVolume = 1f;
        private float _sFXVolume = 1f;

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                if (_musicVolume == value) return;
                _musicVolume = value;

                foreach (var soundPlayer in FindObjectsByType<AbstractSoundPlayer>(FindObjectsSortMode.None))
                {
                    if (soundPlayer.SoundType == SoundTypes.MUSIC)
                    {
                        soundPlayer.UpdateVolume();
                    }
                }
            }
        }

        public float SFXVolume
        {
            get => _sFXVolume;
            set
            {
                if (_sFXVolume == value) return;
                _sFXVolume = value;

                foreach (var soundPlayer in FindObjectsByType<AbstractSoundPlayer>(FindObjectsSortMode.None))
                {
                    if (soundPlayer.SoundType == SoundTypes.SFX)
                    {
                        soundPlayer.UpdateVolume();
                    }
                }
            }
        }
    }

    public static SoundManager Instance;

    public SoundData SoundVolume = new SoundData();

    void Start()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 Instance of SoundManager objects");
        Instance = this;

        DontDestroyOnLoad(this);
    }

    public float GetCurrentSoundTypeVolume(SoundTypes soundType)
    {
        switch (soundType)
        {
            case SoundTypes.SFX:
                return SoundVolume.SFXVolume;
            case SoundTypes.MUSIC:
                return SoundVolume.MusicVolume;
        }
        throw new UnityException("not found soundData value for SoundType: " + soundType.ToString());
    }

    public void SaveSoundToJSON()
    {
        JSONFileManager.SaveJSON(JSONFileManager.Instance.SoundFileName, JsonUtility.ToJson(SoundVolume));
    }
    public void LoadSoundFromJSON()
    {
        string volumeDataStr = JSONFileManager.ReadJSON(JSONFileManager.Instance.SoundFileName);
        if (volumeDataStr == null || volumeDataStr == "") return;

        SoundVolume = JsonUtility.FromJson<SoundData>(volumeDataStr);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

}
