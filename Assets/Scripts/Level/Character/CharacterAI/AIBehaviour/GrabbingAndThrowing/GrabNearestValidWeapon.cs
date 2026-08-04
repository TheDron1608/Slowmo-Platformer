using UnityEngine;

public class GrabNearestValidWeapon : AbstractAIGrabbingAndThrowing
{
    private void FixedUpdate()
    {
        if (_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null && _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != CharComponents.CharacterHolding.CurrentHoldObject)
        {
            CharComponents.CharacterHolding.TryGrab(_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable, true);
        }
    }
}
