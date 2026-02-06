public class OnAbleToDefendBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public override bool StateBehaviourCondition()
    {
        return
            CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<Shield>() != null &&
            CharComponents.CharacterAttacking.IsAbleToShield;
    }
}
