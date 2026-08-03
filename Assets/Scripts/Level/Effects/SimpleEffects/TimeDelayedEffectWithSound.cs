
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TimeDelayedEffectWithSound : TimeDelayedEffect
{
    const float MAX_PITCH = 3.5f;
    const float MAX_FREQ_OFFSET = 0.25f;

    public SoundPlayer SoundPlayer;
    public float MaxSoundDelay = 1f;
    public float MinSoundDelay = 0.05f;
    public bool AllowCombineSoundPlayers = false;

    private float _timeSinceLastSound = 9999f;

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        _timeSinceLastSound += Time.fixedDeltaTime;
        float currentProgress = _timeSpent / math.max(Delay - MAX_FREQ_OFFSET, 0.001f);
        if (_timeSinceLastSound > math.lerp(MaxSoundDelay, MinSoundDelay, currentProgress))
        {
            _timeSinceLastSound = 0f;
            SoundPlayer.Pitch = math.lerp(1f, MAX_PITCH, currentProgress);
            SoundPlayer.PlaySound();
        }
    }

    protected override void OnRemove()
    {
        SoundPlayer.BreakAllSounds();
        base.OnRemove();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            SoundPlayer.DefaultSound == (other as TimeDelayedEffectWithSound).SoundPlayer.DefaultSound &&
            MaxSoundDelay == (other as TimeDelayedEffectWithSound).MaxSoundDelay &&
            MinSoundDelay == (other as TimeDelayedEffectWithSound).MinSoundDelay;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            (AllowCombineSoundPlayers || !affectWho.GetHasEffect<TimeDelayedEffectWithSound>());
    }
}