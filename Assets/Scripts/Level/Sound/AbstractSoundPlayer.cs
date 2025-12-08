using UnityEngine;

public abstract class AbstractSoundPlayer : MonoBehaviour
{
    protected const float MIN_VOLUME = 0.01f;

    public Sound DefaultSound;
    public float Pitch = 1f;
    public float Volume = 1f;
    public SoundManager.SoundTypes SoundType = SoundManager.SoundTypes.SFX;
    [SerializeField] protected AudioSource _audioSource;

    private float _dynamicVolumeMultiplier = 1f;

    public float DynamicVolumeMultiplier
    {
        get => _dynamicVolumeMultiplier;
        set => _dynamicVolumeMultiplier = value;
    }

    public void PlaySound(bool loop = false, Vector2? audioPoint = null)
    {
        PlaySound(DefaultSound, loop, audioPoint);
    }

    public void BreakAllSounds()
    {
        _audioSource.Stop();
    }

    public bool GetIsPlaying()
    {
        return _audioSource.isPlaying;
    }

    protected virtual float CalculateVolume()
    {
        return Volume * DynamicVolumeMultiplier * (SoundManager.Instance?.GetCurrentSoundTypeVolume(SoundType) ?? 1f);
    }

    public abstract void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null);
}