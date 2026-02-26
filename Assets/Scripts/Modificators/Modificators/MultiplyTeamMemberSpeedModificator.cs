using Unity.Mathematics;

public class MultiplyTeamMemberSpeedModificator : AbstractCharactersModificator
{
    public float SpeedMultiplier = 1f;
    public float JumpMultiplier = 1f;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterMoving.Speed *= SpeedMultiplier * ModificatorMultiplier;
        character.CharacterJumping.JumpForce *= JumpMultiplier * ModificatorMultiplier;
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.CharacterMoving.Speed /= SpeedMultiplier * ModificatorMultiplier;
        character.CharacterJumping.JumpForce /= JumpMultiplier * ModificatorMultiplier;
    }
}