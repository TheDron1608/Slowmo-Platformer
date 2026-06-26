
public class InstantStunRecover : AbstractCharacterEffect, IEntireCharacterEffect
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedObject.RemoveEffect<HardStun>();
        AffectedObject.RemoveEffect<MinorStun>();

        if (
            AffectedCharacter.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.FALLEN_ON_FLOOR ||
            AffectedCharacter.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.FALLING_IN_AIR ||
            AffectedCharacter.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.MINOR_STUN
            )
        {
            AffectedCharacter.CharacterVisual.ForceResetBusyAnimation();
        }
    }
}