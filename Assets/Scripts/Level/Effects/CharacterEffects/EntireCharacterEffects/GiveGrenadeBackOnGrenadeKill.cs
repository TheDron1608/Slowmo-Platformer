using System.Collections.Generic;
using UnityEngine;

public class GiveGrenadeBackOnGrenadeKill : AbstractOnKillEffect
{
    protected override void OnKill(IEffectApplier.OnEffectAppliedEventArgs killInfo)
    {
        if (
            AffectedCharacter.CharacterHolding.CurrentHoldObject == null &&
            TryGetGrenadeFromAppliers(killInfo.Appliers, out Holdable holdable)
            )
        {
            Holdable newH = AffectedCharacter.CharacterHolding.GiveNewHoldable(holdable);

            if (newH.TryGetComponent(out OnInteractArmGrenade newGreande))
            {
                newGreande.Armed = false;
            }
        }
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