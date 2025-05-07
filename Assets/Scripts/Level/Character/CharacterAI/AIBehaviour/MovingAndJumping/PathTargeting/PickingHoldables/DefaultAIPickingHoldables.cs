using System.Linq;
using UnityEngine;

public class DefaultAIPickingHoldables : AbstractAIPathfindingMovingAndJumping
{
    protected override void UpdatePathTarget()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject == null ||
            (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon holdRangedWeapon) && holdRangedWeapon.GetIsOutOfAmmo()) &&
            CharComponents.CharacterAIManager.NearestPrefferedHoldable.NearestPrefferedHoldable != null
            )
        {
            CharComponents.CharacterAIManager.AIPathfinding.PathTarget = CharComponents.CharacterAIManager.NearestPrefferedHoldable.NearestPrefferedHoldable.transform.position;
        }
        else
        {
            CharComponents.CharacterAIManager.AIPathfinding.PathTarget = null;
        }
    }
}
