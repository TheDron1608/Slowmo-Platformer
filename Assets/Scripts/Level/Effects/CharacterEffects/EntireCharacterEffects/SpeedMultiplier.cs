public class SpeedMultiplier : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float SpeedMultiplierAmount = 1f;

    protected override void OnApply()
    {
        base.OnApply();
        AffectedCharacter.CharacterMoving.Speed *= SpeedMultiplierAmount;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        AffectedCharacter.CharacterMoving.Speed /= SpeedMultiplierAmount;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && SpeedMultiplierAmount == (other as SpeedMultiplier).SpeedMultiplierAmount;
    }
}
