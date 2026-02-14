using System.Collections.Generic;

public class ForceGrabNearestValidWeapon : AbstractAIGrabbingAndThrowing
{
    public List<AbstractEffect> EffectsOnOldHolder = new();
    private void FixedUpdate()
    {
        if (
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null && 
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.CurrentHolder != CharComponents.CharacterHolding &&
            CharComponents.CharacterHolding.IsAbleToGrabObjects && 
            CharComponents.CharacterHolding.IsAbleToHoldObjects
            )
        {
            _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.CurrentHolder?.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnOldHolder, this);
            CharComponents.CharacterHolding.ForceGrab(_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable);
        }
    }
}
