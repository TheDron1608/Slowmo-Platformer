
using UnityEngine;

public class CatchDangerousHoldables : AbstractAIGrabbingAndThrowing
{
    private void FixedUpdate()
    {
        if (
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null && 
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.CurrentOrLastHolder != CharComponents.CharacterHolding &&
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.GetIsDangerouslyFast()
            )
        {
            CharComponents.CharacterHolding.TryGrab(_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable, true);
        }
    }
}
