using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DefaultAIPathfindingDependsOnCurrentWeapon : DefaultAIPathfinding
{
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

        base.OnUpdateInfo();
    }
}