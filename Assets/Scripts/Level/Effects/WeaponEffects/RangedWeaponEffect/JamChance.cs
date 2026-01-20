
using System;

public class JamChance : AbstractRangedWeaponEffect, ITriggerableEffect
{
    public float Chance;

    public event EventHandler OnTriggered;

    protected override void OnApply()
    {
        base.OnApply();

        RangedWeapon.JamChance += Chance;
        RangedWeapon.OnAttackFailed += RangedWeapon_OnAttackFailed;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        RangedWeapon.JamChance -= Chance;
        RangedWeapon.OnAttackFailed -= RangedWeapon_OnAttackFailed;
    }

    private void RangedWeapon_OnAttackFailed(object sender, EventArgs e)
    {
        if (
            !((RangedWeapon)sender).GetIsOutOfAmmo() && 
            !((RangedWeapon)sender).IsReloading &&
            !((RangedWeapon)sender).IsInCooldown
            )
        {
            OnTriggered?.Invoke(this, EventArgs.Empty);
        }
    }
}