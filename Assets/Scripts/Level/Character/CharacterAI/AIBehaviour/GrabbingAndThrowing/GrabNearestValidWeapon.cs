public class GrabNearestValidWeapon : AbstractAIGrabbingAndThrowing
{
    private void FixedUpdate()
    {
        if (_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null && _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.CurrentHolder != CharComponents.CharacterHolding)
        {
            CharComponents.CharacterHolding.TryGrab(_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable, true);
        }
    }
}
