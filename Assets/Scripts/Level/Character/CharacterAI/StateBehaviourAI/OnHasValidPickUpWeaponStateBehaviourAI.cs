public class OnHasValidPickUpWeaponStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public override bool StateBehaviourCondition()
    {
        return PrefferedHoldable.NearestPrefferedHoldable != null;
    }
}
