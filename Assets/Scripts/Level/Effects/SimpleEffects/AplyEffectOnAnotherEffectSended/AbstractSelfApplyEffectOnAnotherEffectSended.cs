using System;
using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public abstract class AbstractSelfApplyEffectOnAnotherEffectSended : AbstractEffect, IMultiplierableEffect, ITriggerableEffect
{
    public AbstractEffect SelfApplyEffect;

    public event EventHandler OnTriggered;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        AffectedObject.GetComponent<IEffectApplier>().OnEffectApplied += ApplyEffectOnAnotherEffectSended_OnEffectApplied;
    }

    private void ApplyEffectOnAnotherEffectSended_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (EffectIsValidToTriggerCondition(e.Effect))
        {
            AffectedObject.ApplyEffect(SelfApplyEffect, null, EffectMultiplier, true);
            OnTriggered?.Invoke(this, EventArgs.Empty);
        }
    }

    protected abstract bool EffectIsValidToTriggerCondition(AbstractEffect effect);

    private void OnDestroy()
    {
        AffectedObject.GetComponent<IEffectApplier>().OnEffectApplied -= ApplyEffectOnAnotherEffectSended_OnEffectApplied;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.TryGetComponent(out IEffectApplier eApplier);
    }

    public override List<AbstractEffect> GetSelfIncludeIncomingEffects()
    {
        return NumberMath.MergeLists(base.GetSelfIncludeIncomingEffects(), SelfApplyEffect.GetSelfIncludeIncomingEffects());
    }

    public override bool Equals(AbstractEffect other)
    {
        return 
            base.Equals(other) && 
            (other as AbstractSelfApplyEffectOnAnotherEffectSended).SelfApplyEffect.Equals(SelfApplyEffect);
    }
}
