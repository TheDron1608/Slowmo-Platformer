using System.Collections.Generic;
using UnityEngine;

public class GiveGrenadeBackOnGrenadeKill : AbstractOnKillEffect
{
    private Holdable _lastGrenadeCheckResult;
    protected override void OnKill(IEffectApplier.OnEffectAppliedEventArgs killInfo)
    {
        Holdable newH = AffectedCharacter.CharacterHolding.GiveNewHoldable(_lastGrenadeCheckResult);

        if (newH.TryGetComponent(out OnInteractArmGrenade newGreande))
        {
            newGreande.Armed = false;
        }
    }

    protected override bool KillCondition(IEffectApplier.OnEffectAppliedEventArgs e)
    {
        return 
            base.KillCondition(e) &&
            AffectedCharacter.CharacterHolding.CurrentHoldObject == null &&
            TryGetGrenadeFromAppliers(e.Appliers, out _lastGrenadeCheckResult);
    }

    private bool TryGetGrenadeFromAppliers(List<IEffectApplier> appliers, out Holdable holdable)
    {
        holdable = default;

        foreach (var applier in appliers)
        {
            if (
                (applier as MonoBehaviour).TryGetComponent(out OnInteractArmGrenade grenadeApplier) &&
                (applier as MonoBehaviour).TryGetComponent(out Holdable holdableApplier)
                )
            {
                holdable = holdableApplier.OriginalPrefab.GetComponent<Holdable>();
                return true;
            }
            else if (
                (applier as MonoBehaviour).TryGetComponent(out AbstractProjectile projectileApplier) &&
                (projectileApplier.RememberWeapon?.TryGetComponent(out OnInteractArmGrenade rememberGrenade) ?? false) &&
                rememberGrenade.TryGetComponent(out Holdable rememberHoldable)
                )
            {
                holdable = rememberHoldable;
                return true;
            }
        }

        return false;
    }
}