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
        public float MusicVolume = 1f;
        public float SFXVolume = 1f;
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
