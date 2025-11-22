using System.Collections;
using UnityEngine;

public class DelayedEffect : AbstractOverwritingEffect
{
    public float Delay = 1f;
    public AbstractEffect EffectOnFinishDelay;
    public AbstractEffect EffectOnBreakDelay;

    private float _timeSpent = 0f;

    protected override void OnApply()
    {
        base.OnApply();

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
            EffectOnFinishDelay.Equals(other) ||
            (
                base.Equals(other) &&
                EffectOnFinishDelay == (other as DelayedEffect).EffectOnFinishDelay &&
                EffectOnBreakDelay == (other as DelayedEffect).EffectOnBreakDelay
            );
    }
}
