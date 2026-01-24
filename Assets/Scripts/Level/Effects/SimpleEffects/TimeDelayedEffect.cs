using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class TimeDelayedEffect : AbstractEffectWithSender, IDelayedEffect, IMultiplierableEffect
{
    public float Delay = 1f;
    public AbstractEffect EffectOnFinishDelay;
    public AbstractEffect EffectOnBreakDelay;

    private float _timeSpent = 0f;
    private float _effectMultiplier = 1f;

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
            AffectedObject.ApplyEffect(EffectOnFinishDelay, Sender, EffectMultiplier);
            RemoveSelf();
        }
    }

    protected override void OnRemove()
    {
        if (_timeSpent < Delay)
        {
            AffectedObject.ApplyEffect(EffectOnBreakDelay, Sender, EffectMultiplier);
        }
        base.OnRemove();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            EffectOnFinishDelay == (other as TimeDelayedEffect).EffectOnFinishDelay &&
            EffectOnBreakDelay == (other as TimeDelayedEffect).EffectOnBreakDelay;
    }

    public override List<AbstractEffect> GetSelfIncludeIncomingEffects()
    {
        return NumberMath.MergeLists(base.GetSelfIncludeIncomingEffects(), EffectOnFinishDelay.GetSelfIncludeIncomingEffects());
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        _timeSpent = 0f;
    }
}
