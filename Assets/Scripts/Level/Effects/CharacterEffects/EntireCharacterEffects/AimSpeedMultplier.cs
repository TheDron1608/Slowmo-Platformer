public class AimSpeedMultplier : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float AimSpeedMultiplier = 1f;

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterAiming.AimSpeed *= AimSpeedMultiplier;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterAiming.AimSpeed /= AimSpeedMultiplier;
    }
}
