using Unity.Mathematics;

public class MultiplyTeamMemberHealthModificator : AbstractCharactersModificator
{
    public float HealthMultiplier = 1f;
    public bool HideHealthTrack = false;

    private bool _oldTrackHealthValue = true;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterHealth.ApplyMaxHealth(character.CharacterHealth.MaxHealth * HealthMultiplier * ModificatorMultiplier, null);
        if (HealthMultiplier > 1f)
        {
            character.CharacterHealth.SetHealth(character.CharacterHealth.CurrentHealth * HealthMultiplier * ModificatorMultiplier, null);
        }

        if (HideHealthTrack && character.UITrack != null && character.UITrack != false)
        {
            _oldTrackHealthValue = character.UITrack.TrackHealth;
            character.UITrack.TrackHealth = false;
            character.UITrack.RefreshAllTracks();
        }
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.CharacterHealth.ApplyMaxHealth(character.CharacterHealth.MaxHealth / HealthMultiplier / ModificatorMultiplier, null);

        if (HideHealthTrack && character.UITrack != null && character.UITrack != _oldTrackHealthValue)
        {
            character.UITrack.TrackHealth = _oldTrackHealthValue;
            character.UITrack.RefreshAllTracks();
        }
    }
}