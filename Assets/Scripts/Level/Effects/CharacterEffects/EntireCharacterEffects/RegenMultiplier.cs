public class RegenMultiplier : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float RegenMult = 1f;

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterHealth.HealMultiplier *= RegenMult;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterHealth.HealMultiplier /= RegenMult;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && RegenMult == (other as RegenMultiplier).RegenMult;
    }
}
