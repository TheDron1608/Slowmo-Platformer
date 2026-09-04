using UnityEngine;

public class StaticSoundPlayer : AbstractSoundPlayer
{
    private float _currentVolumeMult = 1f;
    private Sound _lastPlayedSound = null;

    public Sound LastPlayedSound
    {
        get => _lastPlayedSound;
    }

    private void Start()
    {
        AudioListenerInstance.Instance.OnDestroyed += Instance_OnDestroyed;
    }

    public override void PlaySound(Sound sound, bool loop = false, Vector2? audioPoint = null, float? startTime = null, float volumeMult = 1f)
    {
        if (sound == null || sound.AudioClips.Count == 0 || !_audioSource.enabled || !gameObject.activeSelf) return;

        AudioClip randomClip = NumberMath.PickRandomItem(sound.AudioClips);
        _currentVolumeMult = volumeMult;
        float targetVolume = CalculateVolume();

        _audioSource.loop = loop;
        _audioSource.pitch = Pitch + NumberMath.PickRandomInRangeNoSeed(-sound.RandomPitchSpread, sound.RandomPitchSpread);
        _audioSource.volume = targetVolume;
        _audioSource.transform.SetParent(AudioListenerInstance.Instance.transform);
        _audioSource.transform.localPosition = Vector3.zero;
        _audioSource.clip = randomClip;
        if (startTime.HasValue)
        {
            Debug.Log(startTime);
            if (startTime >= 1f) startTime = 0.99f;
            if (startTime < 0f) startTime = 0f;
            _audioSource.time = startTime.Value * randomClip.length;
        }
        _audioSource.Play();

        _lastPlayedSound = sound;
    }

    protected override float CalculateVolume()
    {
        return base.CalculateVolume() * NumberMath.LimitFloatBetweenZeroAndOne(_currentVolumeMult);
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