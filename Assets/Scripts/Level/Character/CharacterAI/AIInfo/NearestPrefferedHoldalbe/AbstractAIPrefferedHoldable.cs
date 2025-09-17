using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractAIPrefferedHoldable : AbstractAIInfo
{
    public float MaxWeaponDetectRange = 5f;
    public int MinWeaponPriority = 1;
    public bool CanCatchDangerousHoldable = false;

    protected Holdable _nearestPrefferedHoldable = null;

    public Holdable NearestPrefferedHoldable
    {
        get
        {
            TryUpdateInfo();
            return _nearestPrefferedHoldable;
        }
    }

    protected override void OnUpdateInfo()
    {
        Holdable bestHoldable = null;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);

        Transform holdablesContainer = currentLayer.HoldablesContainer.transform;
        foreach (Holdable holdable in holdablesContainer.GetComponentsInChildren<Holdable>())
        {
            if (PickUpCondition(holdable))
            {
                if (
                    bestHoldable == null ||
                    OrderByPattern(bestHoldable, holdable)
                    )
                {
                    bestHoldable = holdable;
                }
            }
        }

        _nearestPrefferedHoldable = bestHoldable;
    }

    protected virtual bool PickUpCondition(Holdable holdable)
    {
        return
            holdable.CurrentHolder == null &&
            holdable.AIPickUpPriority >= MinWeaponPriority &&
            Vector2.Distance(CharComponents.Center.transform.position, holdable.transform.position) <= MaxWeaponDetectRange &&
            (CanCatchDangerousHoldable || !holdable.GetIsDangerouslyFast()) &&
            (!holdable.TryGetComponent(out RangedWeapon rangedWeapon) || !rangedWeapon.GetIsOutOfAmmo()) &&
            Physics2D.Linecast(
                CharComponents.Center.transform.position,
                holdable.TryGetComponent(out Collider2D col) ? GameObjectUtility.GetCenterOfCollider(col) : holdable.transform.position,
                1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).EnviromentLayer
                ).collider == null;
    }

    protected abstract bool OrderByPattern(Holdable oldHoldable, Holdable newHoldable);
}
