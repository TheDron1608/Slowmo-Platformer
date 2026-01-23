using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class TimeDelayedEffect : AbstractEffectWithSender, IDelayedEffect
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
            AffectedObject.OnApplyEffect(EffectOnFinishDelay, Sender);
            RemoveSelf();
        }
    }

    protected override void OnRemove()
    {
        if (_timeSpent < Delay)
        {
            AffectedObject.OnApplyEffect(EffectOnBreakDelay, Sender);
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
