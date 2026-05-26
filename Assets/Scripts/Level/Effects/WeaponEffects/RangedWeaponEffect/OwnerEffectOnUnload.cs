using System;
using Unity.VisualScripting;
using UnityEngine;

public class OwnerEffectOnUnload : AbstractRangedWeaponEffect, ITriggerableEffect
{
    public AbstractEffect OwnerEffect;

    public event EventHandler OnTriggered;

    protected override void OnApply()
    {
        base.OnApply();

        RangedWeapon.OnUnloaded += RangedWeapon_OnLoadChanged;
        if (RangedWeapon is MagReloadingWeapon magReloadingWeapon)
        {
            magReloadingWeapon.OnReloadedBullet += RangedWeapon_OnLoadChanged;
        }
    }

    private void RangedWeapon_OnLoadChanged(object sender, System.EventArgs e)
    {
        if (RangedWeapon.GetIsOutOfAmmo())
        {
            OnTriggered?.Invoke(this, new EventArgs());
            if (RangedWeapon.TryGetComponent(out Holdable holdableWeapon))
            {
                holdableWeapon.CurrentHolder?.CharComponents.CharacterEffectsReceiver.ApplyEffect(OwnerEffect, RangedWeapon);
            }
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (RangedWeapon != null && !RangedWeapon.IsDestroyed())
        {
            RangedWeapon.OnUnloaded -= RangedWeapon_OnLoadChanged;
            if (RangedWeapon is MagReloadingWeapon magReloadingWeapon)
            {
                magReloadingWeapon.OnReloadedBullet -= RangedWeapon_OnLoadChanged;
            }
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.TryGetComponent(out Holdable h);
    }

    public override bool Equals(AbstractEffect other)
    {
        return 
            base.Equals(other) && 
            (OwnerEffect?.Equals((other as OwnerEffectOnUnload).OwnerEffect) ?? OwnerEffect == (other as OwnerEffectOnUnload).OwnerEffect);
    }
}