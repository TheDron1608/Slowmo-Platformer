using UnityEngine;

public abstract class AbstractSoundPlayer : MonoBehaviour
{
    protected const float MIN_VOLUME = 0.01f;

    public Sound DefaultSound;
    public SoundManager.SoundTypes SoundType = SoundManager.SoundTypes.SFX;

    [SerializeField] private float _pitch = 1f;
    [SerializeField] private float _volume = 1f;
    [SerializeField] protected AudioSource _audioSource;

    private float _dynamicVolumeMultiplier = 1f;

    public float Pitch
    {
        get => _pitch;
        set
        {
            if (_pitch == value) return;
            
            if (_audioSource != null)
            {
                _audioSource.pitch = _audioSource.pitch / _pitch * value;
            }

            _pitch = value;
        }
    }

    public float Volume
    {
        get => _volume;
        set
        {
            if (_volume == value) return;

            _volume = value;

            if (_audioSource != null)
            {
                _audioSource.volume = CalculateVolume();
            }
        }
    }

    public float PlayTime
    {
        get => _audioSource.time;
        set => _audioSource.time = value;
    }

    public float DynamicVolumeMultiplier
    {
        get => _dynamicVolumeMultiplier;
        set
        {
            if (_dynamicVolumeMultiplier == value) return;

            _dynamicVolumeMultiplier = value;

            if (_audioSource != null)
            {
                _audioSource.volume = CalculateVolume();
            }
        }
    }

    public float CurrentClipDuration
    {
        get => _audioSource.isPlaying ? _audioSource.clip.length : 0f;
    }

    public void PlaySound(bool loop = false, Vector2? audioPoint = null, float? startTime = null)
    {
        PlaySound(DefaultSound, loop, audioPoint, startTime);
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

    public abstract void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null, float? startTime = null);
}