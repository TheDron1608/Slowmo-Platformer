
using UnityEngine;

public class SetClumsyAttack : AbstractCharacterEffect, IEntireCharacterEffect
{
    public bool Value;

    private bool _oldClumsyMelee;
    private bool _oldClumsyRanged;
    private bool _oldClumsyShield;

    protected override void OnApply()
    {
        base.OnApply();

        _oldClumsyMelee = AffectedCharacter.CharacterClumsyness.ClumsyMeleeAttack;
        _oldClumsyRanged = AffectedCharacter.CharacterClumsyness.ClumsyRangedAttack;
        _oldClumsyShield = AffectedCharacter.CharacterClumsyness.ClumsyShielding;

        AffectedCharacter.CharacterClumsyness.ClumsyMeleeAttack = Value;
        AffectedCharacter.CharacterClumsyness.ClumsyRangedAttack = Value;
        AffectedCharacter.CharacterClumsyness.ClumsyShielding = Value;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterClumsyness.ClumsyMeleeAttack = _oldClumsyMelee;
        AffectedCharacter.CharacterClumsyness.ClumsyRangedAttack = _oldClumsyRanged;
        AffectedCharacter.CharacterClumsyness.ClumsyShielding = _oldClumsyShield;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<SetClumsyAttack>();
    }
}