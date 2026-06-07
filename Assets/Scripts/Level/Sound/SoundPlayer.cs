using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : AbstractSoundPlayer
{
    const float MIN_VOLUME_DISTANCE = 18.5f;

    private AudioSource _clipPointSource = null;

    public override void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null, float? startTime = null)
    {
        if (sound == null || !_audioSource.enabled || !gameObject.activeSelf) return;

        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        float targetVolume = CalculateVolume();

        if (audioPoint.HasValue)
        {
            if (_clipPointSource == null)
            {
                _clipPointSource = Instantiate(_audioSource);
            }
            else
            {
                _clipPointSource.Stop();
            }
            _clipPointSource.transform.position = audioPoint.Value;
            _clipPointSource.clip = randomClip;
            _clipPointSource.volume = targetVolume;
            _clipPointSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
            _clipPointSource.loop = loop;
            _clipPointSource.Play();
        }
        else
        {
            _audioSource.loop = loop;
            _audioSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
            _audioSource.volume = targetVolume;
            _audioSource.clip = randomClip;
            if (startTime.HasValue)
            {
                _audioSource.time = startTime.Value * randomClip.length;
            }
            _audioSource.Play();
        }
    }

    private void FixedUpdate()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.volume = CalculateVolume();
        }
    }

    protected override float CalculateVolume()
    {
        return 
            base.CalculateVolume() * NumberMath.LimitFloatBetweenZeroAndOne(1f - Vector2.Distance(Camera.main.transform.position, transform.position) / MIN_VOLUME_DISTANCE);
    }

    private void OnEnable()
    {
        BreakAllSounds();
    }

    private void OnDestroy()
    {
        if (_audioSource != null && !_audioSource.IsDestroyed())
        {
            Destroy(_audioSource.gameObject);
        }
    }
}