public class Resurrect : AbstractCharacterEffect, IEntireCharacterEffect
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterHealth.SetHealth(AffectedCharacter.CharacterHealth.MaxHealth, null, null);
    }
}
