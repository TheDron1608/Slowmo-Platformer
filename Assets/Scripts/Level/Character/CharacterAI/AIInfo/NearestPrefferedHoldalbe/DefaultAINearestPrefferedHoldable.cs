using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DefaultAINearestPrefferedHoldable : AbstractAINearestPrefferedHoldable
{
    protected override void OnUpdateInfo()
    {
        Holdable bestHoldable = null;
        float bestDistance = float.MaxValue;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);

        Transform holdablesContainer = currentLayer.HoldablesContainer.transform;
        foreach (Transform holdableTransform in holdablesContainer)
        {
            if (
                !holdableTransform.TryGetComponent(out Holdable holdable) ||
                holdable.AIPickUpPriority < MinWeaponPriority
                )
            {
                continue;
            }

            float distanceToHoldable = Vector2.Distance(CharComponents.transform.position, holdable.transform.position);
            if (
                distanceToHoldable > MaxWeaponDetectRange ||
                (holdable.GetComponent<RangedWeapon>() != null && !CanPickUpRangedWeapon) ||
                (holdable.GetComponent<MeleeWeapon>() != null && !CanPickMeleeWeapon) ||
                (CanPickUpOnlyWhitelistItems && !WhitelistItems.Contains(holdable)) ||
                Physics2D.Linecast(
                    CharComponents.Center.transform.position,
                    holdable.transform.position,
                    1 << currentLayer.EnviromentLayer
                    ).collider == null 
                )
            {
                continue;
            }

            if (
                holdable.AIPickUpPriority > bestHoldable?.AIPickUpPriority ||
                (holdable.AIPickUpPriority == bestHoldable?.AIPickUpPriority && bestDistance < distanceToHoldable)
                )
            {
                bestHoldable = holdable;
                bestDistance = distanceToHoldable;
            }
        }

        _nearestPrefferedHoldable = bestHoldable;
    }
}
