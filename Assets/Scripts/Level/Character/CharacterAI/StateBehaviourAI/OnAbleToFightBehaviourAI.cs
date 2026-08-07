public class OnAbleToFightBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public bool AllowUseGrenades = true;

    public override bool StateBehaviourCondition()
    {
        return
            CharComponents.UnarmedAttacking.Projectile != null ||
            (
                CharComponents.CharacterHolding.CurrentHoldObject != null && 
                (
                    (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) && weapon.GetIsAbleToAttack()) ||
                    (AllowUseGrenades && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out OnInteractArmGrenade grenade))
                )
            );
    }
}
