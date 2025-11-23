using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

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

    [Header("Sound references")]
    public Sound ButtonClickSound;
    public Sound ButtonSelectSound;

    void Start()
    {
        if (Instance != null) throw new UnityException("Limit of 1 Instance of SoundManager objects");
        Instance = this;

        DontDestroyOnLoad(this);
    }

    public void PlaySound(Sound sound)
    {
        AudioSource newSource = FindFirstObjectByType<AudioListener>().gameObject.AddComponent<AudioSource>();


        newSource.resource = sound.AudioClips[UnityEngine.Random.Range(0, sound.AudioClips.Count - 1)];
        newSource.pitch = UnityEngine.Random.Range(1f - sound.RandomPitchSpread, 1f + sound.RandomPitchSpread);
        newSource.volume = (float)SoundVolume.SFXVolume / (float)MaxSoundVolume.SFXVolume;
        newSource.Play();

        StartCoroutine(AwaitSoundPlayerFinishAndDestroy(newSource));
    }

    private IEnumerator AwaitSoundPlayerFinishAndDestroy(AudioSource soundPlayer)
    {

        while (!soundPlayer.IsDestroyed() && soundPlayer.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(soundPlayer);
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
