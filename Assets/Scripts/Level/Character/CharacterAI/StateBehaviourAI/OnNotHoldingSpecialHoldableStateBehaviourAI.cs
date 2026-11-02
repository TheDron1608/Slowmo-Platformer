
public class OnNotHoldingSpecialHoldableStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public override bool StateBehaviourCondition()
    {
        return
            CharacterHoldableSpeicalModificator.Instance != null &&
            CharacterHoldableSpeicalModificator.Instance.CurrentSpecialHoldable != CharComponents.CharacterHolding.CurrentHoldObject &&
            CharacterHoldableSpeicalModificator.Instance.CurrentSpecialHoldable != CharComponents.CharacterHolding.CurrentHolsteredHoldObject;
    }
}