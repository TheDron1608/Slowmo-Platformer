using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class TimeDelayedEffect : AbstractEffectWithSender, IDelayedEffect, IMultiplierableEffect
{
    public float Delay = 1f;
    public List<AbstractEffect> EffectsOnFinishDelay;
    public List<AbstractEffect> EffectsOnBreakDelay;

    private float _effectMultiplier = 1f;

    protected float _timeSpent = 0f;

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
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {
        _timeSpent += Time.deltaTime;

        if (_timeSpent >= Delay)
        {
            AffectedObject.ApplyEffect(EffectsOnFinishDelay, Sender, EffectMultiplier);
            RemoveSelf();
        }
    }

    protected override void OnRemove()
    {
        if (_timeSpent < Delay)
        {
            AffectedObject.ApplyEffect(EffectsOnBreakDelay, Sender, EffectMultiplier);
        }
        base.OnRemove();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            Delay == (other as TimeDelayedEffect).Delay &&
            EffectsOnFinishDelay.TrueForAll(effect => (other as TimeDelayedEffect).EffectsOnFinishDelay.Contains(effect)) &&
            EffectsOnBreakDelay.TrueForAll(effect => (other as TimeDelayedEffect).EffectsOnBreakDelay.Contains(effect));
    }

    public override List<AbstractEffect> GetSelfIncludeIncomingEffects()
    {
        List<AbstractEffect> result = base.GetSelfIncludeIncomingEffects();
        foreach (var effectOnFinish in EffectsOnFinishDelay)
        {
            result.AddRange(effectOnFinish.GetSelfIncludeIncomingEffects());
        }
        foreach (var effectOnBreak in EffectsOnBreakDelay)
        {
            result.AddRange(effectOnBreak.GetSelfIncludeIncomingEffects());
        }

        return result;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        _timeSpent = 0f;
    }
}
