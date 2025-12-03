using System.Linq;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    const float MIN_VOLUME_DISTANCE = 15f;

    public Sound DefaultSound;
    public float Pitch = 1f;
    public float Volume = 1f;

    [SerializeField] private int _maxSourcesPlayingTogether = 1;

    private AudioSource[] _audioSources;
    private float _dynamicVolumeMultiplier = 1f;

    public float DynamicVolumeMultiplier
    {
        get => _dynamicVolumeMultiplier; 
        set => _dynamicVolumeMultiplier = value;
    }

    private void Awake()
    {
        _audioSources = new AudioSource[_maxSourcesPlayingTogether];
        for (int i = 0; i < _maxSourcesPlayingTogether; i++)
        {
            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            _audioSources[i] = newAudioSource;
        }
    }

    public void PlaySound(bool loop = false, Vector2? audioPoint = null)
    {
        PlaySound(DefaultSound, loop, audioPoint);
    }

    public void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null)
    {
        if (sound == null) return;

        AudioSource mostLateTimeAudioSource = _audioSources.First();
        foreach (AudioSource audioSource in _audioSources)
        {
            if (!audioSource.isPlaying)
            {
                PlayAudioSource(audioSource, sound, loop, audioPoint);
                return;
            }
            else if (audioSource.clip != null && mostLateTimeAudioSource.time < audioSource.time)
            {
                mostLateTimeAudioSource = audioSource;
            }
        }
        PlayAudioSource(mostLateTimeAudioSource, sound, loop, audioPoint);
    }

    private void FixedUpdate()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            if (audioSource.isPlaying)
            {
                audioSource.volume = CalculateVolume();
            }
        }
    }

    private void PlayAudioSource(AudioSource audioSource, Sound sound, bool loop, Vector2? audioPoint)
    {
        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        if (audioPoint.HasValue)
        {
            AudioSource.PlayClipAtPoint(
                randomClip, 
                audioPoint.Value,
                CalculateVolume()
                );
        }
        else
        {
            audioSource.loop = loop;
            audioSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
            audioSource.volume = CalculateVolume();
            if (loop)
            {
                audioSource.clip = randomClip;
                audioSource.Play();
            }
            else
            {
                audioSource.PlayOneShot(randomClip);
            }
        }
    }

    public void BreakAllSounds()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.Stop();
        }
    }

    public bool GetIsPlaying()
    {
        return _audioSources.Any(audioSource => audioSource.isPlaying);
    }

    private float CalculateVolume()
    {
        return Volume * DynamicVolumeMultiplier * NumberMath.LimitFloatBetweenZeroAndOne(1f - Vector2.Distance(Camera.main.transform.position, transform.position) / MIN_VOLUME_DISTANCE);
    }
}