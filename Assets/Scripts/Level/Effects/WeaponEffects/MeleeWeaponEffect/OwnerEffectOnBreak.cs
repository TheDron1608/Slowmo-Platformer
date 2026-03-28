using System;
using Unity.VisualScripting;
using UnityEngine;

public class OwnerEffectOnBreak : AbstractMeleeWeaponEffect, ITriggerableEffect
{
    public AbstractEffect OwnerEffect;

    public event EventHandler OnTriggered;

    protected override void OnApply()
    {
        base.OnApply();

        MeleeWeapon.GetComponent<BreakableHoldable>().OnBroken += OwnerEffectOnBreak_OnBroken;
    }

    private void OwnerEffectOnBreak_OnBroken(object sender, MonoBehaviour e)
    {
        OnTriggered?.Invoke(this, EventArgs.Empty);
        MeleeWeapon.GetComponent<Holdable>()?.CurrentHolder?.CharComponents.CharacterEffectsReceiver.ApplyEffect(OwnerEffect, MeleeWeapon);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (MeleeWeapon != null && !MeleeWeapon.IsDestroyed())
        {
            MeleeWeapon.GetComponent<BreakableHoldable>().OnBroken -= OwnerEffectOnBreak_OnBroken;
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.GetComponent<Holdable>() != null && affectWho.GetComponent<BreakableHoldable>() != null;
    }

    public override bool Equals(AbstractEffect other)
    {
        return 
            base.Equals(other) && 
            (OwnerEffect?.Equals((other as OwnerEffectOnBreak).OwnerEffect) ?? OwnerEffect == (other as OwnerEffectOnBreak).OwnerEffect);
    }
}