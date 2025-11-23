public class IgnoreProjectiles : AbstractCharacterEffect, IEntireCharacterEffect
{
    public bool IgnoreMeleeProjectiles = true;
    public bool IgnoreRangedProjectiles = true;

    protected override void OnApply()
    {
        base.OnApply();
        AffectedCharacter.CharacterHealth.HitableByMeleeProjectiles &= !IgnoreMeleeProjectiles;
        AffectedCharacter.CharacterHealth.HitableByRangedProjectiles &= !IgnoreRangedProjectiles;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        AffectedCharacter.CharacterHealth.HitableByMeleeProjectiles |= IgnoreMeleeProjectiles;
        AffectedCharacter.CharacterHealth.HitableByRangedProjectiles |= IgnoreRangedProjectiles;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) &&
            IgnoreMeleeProjectiles == (other as IgnoreProjectiles).IgnoreMeleeProjectiles &&
            IgnoreRangedProjectiles == (other as IgnoreProjectiles).IgnoreRangedProjectiles;
    }
}
