using System.Collections.Generic;
using UnityEngine;

public class TimeDelayedEffect : AbstractOverwritingEffect, IDelayedEffect
{
    public float Delay = 1f;
    public AbstractEffect EffectOnFinishDelay;
    public AbstractEffect EffectOnBreakDelay;

    private float _timeSpent = 0f;

    public float TimeLeft
    {
        get => Delay - _timeSpent;
        set => Delay = _timeSpent + value;
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
            AffectedObject.ApplyEffect(EffectOnFinishDelay, null);
            RemoveSelf();
        }
    }

    protected override void OnRemove()
    {
        if (_timeSpent < Delay)
        {
            AffectedObject.ApplyEffect(EffectOnBreakDelay, null);
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
        return new() { this, EffectOnFinishDelay };
    }
}
