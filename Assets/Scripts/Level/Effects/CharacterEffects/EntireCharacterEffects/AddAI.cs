using System.Collections.Generic;

public class AddAI : AbstractCharacterEffect, IEntireCharacterEffect
{
    public AbstractCharacterStateBehaviourAI ReplaceAI;

    private AbstractCharacterStateBehaviourAI _replacedAI;

    protected override void OnApply()
    {
        base.OnApply();

        _replacedAI = AffectedCharacter.CharacterAIManager.AddStateBehaviourAI(ReplaceAI);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterAIManager.RemoveStateBehaviourAI(_replacedAI);
    }
}
