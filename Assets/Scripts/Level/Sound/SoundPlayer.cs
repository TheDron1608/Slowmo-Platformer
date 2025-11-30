using System.Linq;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public Sound DefaultSound;
    public float Pitch = 1f;
    public float Volume = 1f;

    [SerializeField] private int _maxSourcesPlayingTogether = 1;

    private AudioSource[] _audioSources;

    private void Awake()
    {
        _audioSources = new AudioSource[_maxSourcesPlayingTogether];
        for (int i = 0; i < _maxSourcesPlayingTogether; i++)
        {
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySound()
    {
        PlaySound(DefaultSound);
    }

    public void PlaySound(Sound sound)
    {
        if (sound == null) return;

        AudioSource mostLateTimeAudioSource = _audioSources.First();
        foreach (AudioSource audioSource in _audioSources)
        {
            if (!audioSource.isPlaying)
            {
                PlayAudioSource(audioSource, sound);
                return;
            }
            else if (mostLateTimeAudioSource.time < audioSource.time)
            {
                mostLateTimeAudioSource = audioSource;
            }
        }
        PlayAudioSource(mostLateTimeAudioSource, sound);
    }

    private void PlayAudioSource(AudioSource audioSource, Sound sound)
    {
        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        audioSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
        audioSource.volume = Volume;
        audioSource.PlayOneShot(randomClip);
    }

    public void BreakAllSounds()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.Stop();
        }
    }
}