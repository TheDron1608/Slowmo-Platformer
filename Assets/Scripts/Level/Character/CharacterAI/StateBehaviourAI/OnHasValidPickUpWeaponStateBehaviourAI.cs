public class OnHasValidPickUpWeaponStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public bool DropOldWeapon = true;

    public override bool StateBehaviourCondition()
    {
        return 
            PrefferedHoldable.NearestPrefferedHoldable != null &&
            (!DropOldWeapon || CharComponents.CharacterHolding.CurrentHoldObject == null);
    }
}
