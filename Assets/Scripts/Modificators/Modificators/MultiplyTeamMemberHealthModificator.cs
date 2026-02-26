using Unity.Mathematics;

public class MultiplyTeamMemberHealthModificator : AbstractCharactersModificator
{
    public float HealthMultiplier = 1f;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterHealth.ApplyMaxHealth(character.CharacterHealth.MaxHealth * HealthMultiplier * ModificatorMultiplier, null);
        character.CharacterHealth.SetHealth(character.CharacterHealth.CurrentHealth * HealthMultiplier * ModificatorMultiplier, null, null);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.CharacterHealth.SetHealth(character.CharacterHealth.CurrentHealth / HealthMultiplier / ModificatorMultiplier, null, null);
        character.CharacterHealth.ApplyMaxHealth(character.CharacterHealth.MaxHealth / HealthMultiplier / ModificatorMultiplier, null);
    }
}