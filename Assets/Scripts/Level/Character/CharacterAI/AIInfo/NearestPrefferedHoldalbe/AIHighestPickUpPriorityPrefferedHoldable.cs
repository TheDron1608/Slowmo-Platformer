using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIHighestPickUpPriorityPrefferedHoldable : AbstractAIPrefferedHoldable
{
    public bool CanCatchDangerousHoldable = false;
    protected override void OnUpdateInfo()
    {
        Holdable bestHoldable = null;
        float bestDistance = float.MaxValue;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);

        Transform holdablesContainer = currentLayer.HoldablesContainer.transform;
        foreach (Holdable holdable in holdablesContainer.GetComponentsInChildren<Holdable>())
        {
            float distanceToHoldable = Vector2.Distance(CharComponents.Center.transform.position, holdable.transform.position);

            if (
                holdable.CurrentHolder == null &&
                holdable.AIPickUpPriority >= MinWeaponPriority &&
                distanceToHoldable <= MaxWeaponDetectRange &&
                (CanCatchDangerousHoldable || !holdable.GetIsDangerouslyFast()) &&
                (!holdable.TryGetComponent(out RangedWeapon rangedWeapon) || rangedWeapon.LoadedLivingAmmoLeft > 0 || rangedWeapon.AmmoLeft > 0) &&
                (CanPickUpRangedWeapon || holdable.GetComponent<RangedWeapon>() == null) &&
                (CanPickMeleeWeapon || holdable.GetComponent<MeleeWeapon>() == null) &&
                (!CanPickUpOnlyWhitelistItems || WhitelistItems.Contains(holdable)) &&
                Physics2D.Linecast(
                    CharComponents.Center.transform.position,
                    holdable.transform.position,
                    1 << currentLayer.EnviromentLayer
                    ).collider == null
                )
            {
                if (
                    bestHoldable == null ||
                    holdable.AIPickUpPriority > bestHoldable.AIPickUpPriority ||
                    (holdable.AIPickUpPriority == bestHoldable.AIPickUpPriority && bestDistance < distanceToHoldable)
                    )
                {
                    bestHoldable = holdable;
                    bestDistance = distanceToHoldable;
                }
            }
        }

        _nearestPrefferedHoldable = bestHoldable;
    }
}
