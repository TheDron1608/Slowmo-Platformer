public class GrabNearestValidWeaponIfUnarmed : AbstractAIGrabbingAndThrowing
{
    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject == null &&
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null && 
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.CurrentHolder != CharComponents.CharacterHolding
            )
        {
            CharComponents.CharacterHolding.TryGrab(_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable, true);
        }
    }
}
