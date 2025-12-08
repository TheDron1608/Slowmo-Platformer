using UnityEngine;

public class StaticSoundPlayer : AbstractSoundPlayer
{
    public override void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null)
    {
        if (sound == null) return;

        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        float targetVolume = CalculateVolume();
        if (targetVolume < MIN_VOLUME) return;

        _audioSource.loop = loop;
        _audioSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
        _audioSource.volume = targetVolume;

        if (_audioSource.volume < MIN_VOLUME) return;

        _audioSource.transform.SetParent(Camera.main.transform);
        _audioSource.transform.localPosition = Vector3.zero;

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

    private void OnDestroy()
    {
        Destroy(_audioSource.gameObject);
    }
}