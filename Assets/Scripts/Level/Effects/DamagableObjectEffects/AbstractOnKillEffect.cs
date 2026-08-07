using System;
using UnityEngine;

public abstract class AbstractOnKillEffect : AbstractCharacterEffect, IEntireCharacterEffect, ITriggerableEffect
{
    public event EventHandler OnTriggered;

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.TryGetComponent(out IEffectApplier eApplier);
    }

    protected override void OnApply()
    {
        base.OnApply();
        foreach (IEffectApplier effectApplier in AffectedObject.GetComponents<IEffectApplier>())
        {
            effectApplier.OnEffectApplied += EffectApplier_OnEffectApplied;
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        foreach (IEffectApplier effectApplier in AffectedObject.GetComponents<IEffectApplier>())
        {
            effectApplier.OnEffectApplied -= EffectApplier_OnEffectApplied;
        }
    }

    private void EffectApplier_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (KillCondition(e))
        {
            OnTriggered?.Invoke(this, EventArgs.Empty);
            OnKill(e);
        }
    }

    protected abstract void OnKill(IEffectApplier.OnEffectAppliedEventArgs killInfo);

    protected virtual bool KillCondition(IEffectApplier.OnEffectAppliedEventArgs e)
    {
        return e.Effect is Death;
    }
}
