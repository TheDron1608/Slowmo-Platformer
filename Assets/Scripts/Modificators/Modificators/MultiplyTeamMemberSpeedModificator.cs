using Unity.Mathematics;

public class MultiplyTeamMemberSpeedModificator : AbstractCharactersModificator
{
    public float SpeedMultiplier = 1f;
    public float JumpMultiplier = 1f;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterMoving.Speed *= 1f + (SpeedMultiplier - 1f) * ModificatorMultiplier;
        character.CharacterJumping.JumpForce *= 1f + (JumpMultiplier - 1f) * ModificatorMultiplier;
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.CharacterMoving.Speed /= 1f + (SpeedMultiplier - 1f) * ModificatorMultiplier;
        character.CharacterJumping.JumpForce /= 1f + (JumpMultiplier - 1f) * ModificatorMultiplier;
    }
}