using UnityEngine;

public class MusicExtraSoundPlayer : MonoBehaviour
{
    private AbstractSoundPlayer _soundPlayer;

    private void Awake()
    {
        if (!TryGetComponent(out _soundPlayer)) throw new UnityException("SoundPlayer component not found");
    }

    private void Start()
    {
        UpdateAudioVolume();
        _soundPlayer.PlaySound(true);
    }

    private void Update()
    {
        UpdateAudioVolume();
    }

    private void UpdateAudioVolume()
    {
        _soundPlayer.DynamicVolumeMultiplier = MusicManager.Instance.CurrentMusicVolume;
    }
}