
using UnityEngine;

public class SetClumsyMovement : AbstractCharacterEffect, IEntireCharacterEffect
{
    public bool Value;

    private bool _oldClumsyMoving;
    private bool _oldClumsyJumping;

    protected override void OnApply()
    {
        base.OnApply();

        _oldClumsyMoving = AffectedCharacter.CharacterClumsyness.ClumsyMovement;
        _oldClumsyJumping = AffectedCharacter.CharacterClumsyness.ClumsyJumping;

        AffectedCharacter.CharacterClumsyness.ClumsyMovement = Value;
        AffectedCharacter.CharacterClumsyness.ClumsyJumping = Value;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterClumsyness.ClumsyMovement = _oldClumsyMoving;
        AffectedCharacter.CharacterClumsyness.ClumsyJumping = _oldClumsyJumping;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<SetClumsyMovement>();
    }
}