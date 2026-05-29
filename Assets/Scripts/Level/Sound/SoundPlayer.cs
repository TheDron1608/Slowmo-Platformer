using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : AbstractSoundPlayer
{
    const float MIN_VOLUME_DISTANCE = 15f;

    public override void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null, float? startTime = null)
    {
        if (sound == null || !_audioSource.enabled || !gameObject.activeSelf) return;

        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        float targetVolume = CalculateVolume();

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
}