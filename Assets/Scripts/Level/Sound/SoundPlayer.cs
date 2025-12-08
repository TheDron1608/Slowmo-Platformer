using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : AbstractSoundPlayer
{
    const float MIN_VOLUME_DISTANCE = 15f;

    public override void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null)
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