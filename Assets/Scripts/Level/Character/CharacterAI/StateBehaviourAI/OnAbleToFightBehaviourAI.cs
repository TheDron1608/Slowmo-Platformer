public class OnAbleToFightBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public override bool StateBehaviourCondition()
    {
        return
            CharComponents.CharacterAttacking.UnarmedAttackProjectile != null ||
            CharComponents.CharacterHolding.CurrentHoldObject != null && 
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) && 
            weapon.GetIsAbleToAttack();
    }
}
