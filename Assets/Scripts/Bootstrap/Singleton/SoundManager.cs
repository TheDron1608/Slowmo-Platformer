using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    const string MASTER_PITCH_PROP_NAME = "MasterPitch";
    const string MUSIC_VOLUME_PROP_NAME = "GameplayMusicVolume";
    const float MAX_SLOWTIME_PITCH = 0.5f;
    const float SLOWTIME_PITCH_AFFECTION_MULT = 2.5f;
    const float VOLUME_FLOAT_TO_DECEBEL_STEP = 35f;

    [Serializable]
    public class SoundData
    {
        const string MUSIC_VOLUME_PROP_NAME = "MusicVolume";
        const string SFX_VOLUME_PROP_NAME = "SFXVolume";

        public float MusicVolume = 1f;
        public float SFXVolume = 1f;

        public void ApplyChanges()
        {
            Instance.MainMixer.SetFloat(MUSIC_VOLUME_PROP_NAME, MathF.Log10(math.max(MusicVolume, 0.00001f)) * VOLUME_FLOAT_TO_DECEBEL_STEP);
            Instance.MainMixer.SetFloat(SFX_VOLUME_PROP_NAME, MathF.Log10(math.max(SFXVolume, 0.00001f)) * VOLUME_FLOAT_TO_DECEBEL_STEP);
        }
    }

    public static SoundManager Instance;

    public SoundData SoundVolume = new SoundData();

    public AudioMixer MainMixer;

    private float _masterPitch = 1f;
    private float _gameplayMusicVolume = 1f;

    public float MasterPitch
    {
        get => _masterPitch;
        private set
        {
            if (_masterPitch == value) return;

            MainMixer.SetFloat(MASTER_PITCH_PROP_NAME, value);
            _masterPitch = value;
        }
    }

    public float GameplayMusicVolume
    {
        get => _gameplayMusicVolume;
        set
        {
            if (_gameplayMusicVolume == value) return;

            MainMixer.SetFloat(MUSIC_VOLUME_PROP_NAME, MathF.Log10(math.max(value, 0.00001f)) * VOLUME_FLOAT_TO_DECEBEL_STEP);
            _gameplayMusicVolume = value;
        }
    }

    void Start()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 Instance of SoundManager objects");
        Instance = this;

        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (TimeManager.Instance != null)
        {
            MasterPitch = math.lerp(1f, MAX_SLOWTIME_PITCH, NumberMath.LimitFloatBetweenZeroAndOne(TimeManager.Instance.TempSlowTimeLeft * SLOWTIME_PITCH_AFFECTION_MULT));
        }
        else
        {
            MasterPitch = 1f;
        }
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
