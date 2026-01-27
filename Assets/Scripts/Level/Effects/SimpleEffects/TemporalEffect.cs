using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class TemporalEffect : AbstractEffectWithSender, IDelayedEffect, IMultiplierableEffect
{
    public float Delay = 1f;
    public AbstractEffect Effect;

    private float _timeSpent = 0f;
    private float _effectMultiplier = 1f;
    private AbstractEffect _currentTempEffect = null;

    public float TimeLeft
    {
        get => Delay - _timeSpent;
        set => Delay = _timeSpent + value;
    }

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    public float TimeSpent
    {
        get => _timeSpent;
    }

    private void FixedUpdate()
    {
        _timeSpent += Time.deltaTime;

        if (_timeSpent >= Delay)
        {
            AffectedObject.RemoveEffect(_currentTempEffect);
            RemoveSelf();
        }
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        _currentTempEffect = AffectedObject.ApplyEffect(Effect, sender, EffectMultiplier);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (_currentTempEffect != null) AffectedObject.RemoveEffect(_currentTempEffect);
    }

    public override bool Equals(AbstractEffect other)
    {
        return 
            base.Equals(other) &&
            Delay == (other as TemporalEffect).Delay &&
            Effect.Equals((other as TemporalEffect).Effect);
    }

    public override List<AbstractEffect> GetSelfIncludeIncomingEffects()
    {
        List<AbstractEffect> result = base.GetSelfIncludeIncomingEffects();
        result.Add(Effect);
        return result;
    }
}
