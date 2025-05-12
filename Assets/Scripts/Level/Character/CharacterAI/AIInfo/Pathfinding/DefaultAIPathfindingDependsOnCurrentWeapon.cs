using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DefaultAIPathfindingDependsOnCurrentWeapon : DefaultAIPathfinding
{
    public float MinDistanceToTargetWithRangedWeapon = 3f;

    protected override void OnUpdateInfo()
    {
        if (
            (CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<MeleeWeapon>() != null && CharComponents.CharacterClumsyness.ClumsyMeleeAttack) ||
            (CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<RangedWeapon>() != null && CharComponents.CharacterClumsyness.ClumsyRangedAttack)
            )
        {
            CanJumpToTarget = false;
        }
        else
        {
            CanJumpToTarget = true;
        }

        if (PathTarget.HasValue && Vector2.Distance(PathTarget.Value, CharComponents.transform.position) < MinDistanceToTargetWithRangedWeapon)
        {
            PathTarget = CharComponents.transform.position;
        }

        base.OnUpdateInfo();
    }
}