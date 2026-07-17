public class ApplyEffectOnBreakArmor : AbstractCharacterEffect, IEntireCharacterEffect, IMultiplierableEffect
{
    public AbstractEffect EffectOnBreak;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterAttacking.OnEffectApplied += CharacterAttacking_OnEffectApplied;
    }

    private void CharacterAttacking_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (e.Effect is PierceArmor)
        {
            AffectedObject.ApplyEffect(EffectOnBreak, null, EffectMultiplier);
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterAttacking.OnEffectApplied += CharacterAttacking_OnEffectApplied;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && EffectOnBreak == (other as ApplyEffectOnBreakArmor).EffectOnBreak;
    }
}
