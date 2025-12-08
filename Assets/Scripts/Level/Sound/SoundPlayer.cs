using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    const float MIN_VOLUME_DISTANCE = 15f;
    const float MIN_VOLUME = 0.01f;

    public Sound DefaultSound;
    public float Pitch = 1f;
    public float Volume = 1f;
    public bool IsPropaginatable = true;

    private AudioSource _audioSource;
    private float _dynamicVolumeMultiplier = 1f;

    public float DynamicVolumeMultiplier
    {
        get => _dynamicVolumeMultiplier; 
        set => _dynamicVolumeMultiplier = value;
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(bool loop = false, Vector2? audioPoint = null)
    {
        PlaySound(DefaultSound, loop, audioPoint);
    }

    public void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null)
    {
        if (sound == null) return;

        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        float targetVolume = CalculateVolume();
        if (targetVolume < MIN_VOLUME) return;

        if (audioPoint.HasValue)
        {
            AudioSource.PlayClipAtPoint(
                randomClip,
                audioPoint.Value,
                targetVolume
                );
        }
        else
        {
            _audioSource.loop = loop;
            _audioSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
            _audioSource.volume = targetVolume;

            if (_audioSource.volume < MIN_VOLUME) return;

            if (loop)
            {
                _audioSource.clip = randomClip;
                _audioSource.Play();
            }
            else
            {
                _audioSource.PlayOneShot(randomClip);
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsPropaginatable && _audioSource.isPlaying)
        {
            _audioSource.volume = CalculateVolume();
        }
    }

    public void BreakAllSounds()
    {
        _audioSource.Stop();
    }

    public bool GetIsPlaying()
    {
        return _audioSource.isPlaying;
    }

    private float CalculateVolume()
    {
        if (IsPropaginatable)
        {
            return Volume * DynamicVolumeMultiplier *
                NumberMath.LimitFloatBetweenZeroAndOne(1f - Vector2.Distance(Camera.main.transform.position, transform.position) / MIN_VOLUME_DISTANCE);
        }
        else
        {
            return Volume * DynamicVolumeMultiplier;
        }
    }
}