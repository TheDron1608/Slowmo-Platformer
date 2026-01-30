using UnityEngine;

public class Telekinesis : AbstractCharacterEffect, IEntireCharacterEffect
{
    protected override void OnApply()
    {
        base.OnApply();
        AffectedCharacter.CharacterHolding.Telekinesis = true;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        AffectedCharacter.CharacterHolding.Telekinesis = false;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<Telekinesis>();
    }
}
