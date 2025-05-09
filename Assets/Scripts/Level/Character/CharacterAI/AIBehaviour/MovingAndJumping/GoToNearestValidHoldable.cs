using System.Linq;
using UnityEngine;

public class GoToNearestValidHoldable : AbstractAIPathfindingMovingAndJumping
{
    protected override void UpdatePathTarget()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject == null ||
            (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon holdRangedWeapon) && holdRangedWeapon.GetIsOutOfAmmo()) &&
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestPrefferedHoldable.NearestPrefferedHoldable != null
            )
        {
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.AIPathfinding.PathTarget = CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestPrefferedHoldable.NearestPrefferedHoldable.transform.position;
        }
        else
        {
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.AIPathfinding.PathTarget = null;
        }
    }
}
