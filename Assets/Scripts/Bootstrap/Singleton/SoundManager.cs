using System;
using UnityEngine;
using static SoundManager;

public class SoundManager : MonoBehaviour
{
    [Serializable]
    public class SoundData
    {
        public int MusicVolume = 5;
        public int SFXVolume = 5;
    }

    public static SoundManager Instance;

    public SoundData SoundVolume = new SoundData();
    public SoundData MaxSoundVolume = new SoundData();

    void Start()
    {
        if (Instance != null) throw new UnityException("Limit of 1 Instance of SoundManager objects");
        Instance = this;

        DontDestroyOnLoad(this);
    }


    private void OnDestroy()
    {
        Instance = null;
    }

    public void SaveSoundToJSON()
    {
        JSONFileManager.SaveJSON(JSONFileManager.Instance.SoundFileName, JsonUtility.ToJson(SoundVolume));
    }
    public void LoadSoundFromJSON()
    {
        string volumeDataStr = JSONFileManager.ReadJSON(JSONFileManager.Instance.SoundFileName);
        if (volumeDataStr == "") return;

        SoundVolume = JsonUtility.FromJson<SoundData>(volumeDataStr);
    }
}
