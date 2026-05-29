using UnityEngine;

public class StaticSoundPlayer : AbstractSoundPlayer
{
    private void Start()
    {
        AudioListenerInstance.Instance.OnDestroyed += Instance_OnDestroyed;
    }

    public override void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null, float? startTime = null)
    {
        if (sound == null || !_audioSource.enabled || !gameObject.activeSelf) return;

        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        float targetVolume = CalculateVolume();

        _audioSource.loop = loop;
        _audioSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
        _audioSource.volume = targetVolume;
        _audioSource.transform.SetParent(AudioListenerInstance.Instance.transform);
        _audioSource.transform.localPosition = Vector3.zero;
        _audioSource.clip = randomClip;
        if (startTime.HasValue)
        {
            _audioSource.time = startTime.Value * randomClip.length;
        }
        _audioSource.Play();

    }

    private void Instance_OnDestroyed(object sender, System.EventArgs e)
    {
        _audioSource.transform.SetParent(transform);
        _audioSource.transform.localPosition = Vector3.zero;
        if (AudioListenerInstance.Instance != null)
        {
            AudioListenerInstance.Instance.OnDestroyed -= Instance_OnDestroyed;
        }
    }

    private void OnDestroy()
    {
        if (AudioListenerInstance.Instance != null)
        {
            AudioListenerInstance.Instance.OnDestroyed -= Instance_OnDestroyed;
        }
        Destroy(_audioSource.gameObject);
    }
}