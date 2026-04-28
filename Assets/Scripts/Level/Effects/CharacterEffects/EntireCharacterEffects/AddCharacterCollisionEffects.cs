using System.Collections.Generic;

public class AddCharacterCollisionEffects : AbstractCharacterEffect, IEntireCharacterEffect
{
    public List<AbstractEffect> EffectsOnHitOtherCharacters = new();
    public List<AbstractEffect> SelfEffectsOnHitOtherCharacters = new();
    public bool CanHitWhileHardStunned = true;
    public bool CanHitWhileMoving = false;
    public bool CanHitWhileRolling = false;

    private bool _oldCanHitWhileHardStunned = true;
    private bool _oldCanHitWhileMoving = false;
    private bool _oldCanHitWhileRolling = false;

    protected override void OnApply()
    {
        base.OnApply();

        _oldCanHitWhileHardStunned = AffectedCharacter.CharacterCollision.CanHitWhileHardStnned;
        _oldCanHitWhileMoving = AffectedCharacter.CharacterCollision.CanHitWhileMoving;
        _oldCanHitWhileRolling = AffectedCharacter.CharacterCollision.CanHitWhileRolling;

        AffectedCharacter.CharacterCollision.CanHitWhileHardStnned |= CanHitWhileHardStunned;
        AffectedCharacter.CharacterCollision.CanHitWhileMoving |= CanHitWhileMoving;
        AffectedCharacter.CharacterCollision.CanHitWhileRolling |= CanHitWhileRolling;

        AffectedCharacter.CharacterCollision.EffectsOnHitOtherCharacters.AddRange(EffectsOnHitOtherCharacters);
        AffectedCharacter.CharacterCollision.SelfEffectsOnHitOtherCharacters.AddRange(SelfEffectsOnHitOtherCharacters);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterCollision.CanHitWhileHardStnned = _oldCanHitWhileHardStunned;
        AffectedCharacter.CharacterCollision.CanHitWhileMoving = _oldCanHitWhileMoving;
        AffectedCharacter.CharacterCollision.CanHitWhileRolling = _oldCanHitWhileRolling;

        NumberMath.RemoveListMultiItems(AffectedCharacter.CharacterCollision.EffectsOnHitOtherCharacters, EffectsOnHitOtherCharacters);
        NumberMath.RemoveListMultiItems(AffectedCharacter.CharacterCollision.SelfEffectsOnHitOtherCharacters, SelfEffectsOnHitOtherCharacters);
    }
}
