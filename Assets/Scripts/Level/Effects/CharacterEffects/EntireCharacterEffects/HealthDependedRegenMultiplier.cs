using Unity.Mathematics;

public class HealthDependedRegenMultiplier : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float MaxRegenMult = 1f;

    private float _currentRegenMult = 1f;

    private void FixedUpdate()
    {
        float targetRegenMult = math.lerp(
            MaxRegenMult,
            1f,
            NumberMath.LimitFloatBetweenZeroAndOne(AffectedCharacter.CharacterHealth.CurrentHealth / AffectedCharacter.CharacterHealth.MaxHealth)
            );

        AffectedCharacter.CharacterHealth.HealMultiplier = AffectedCharacter.CharacterHealth.HealMultiplier / _currentRegenMult * targetRegenMult;

        _currentRegenMult = targetRegenMult;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterHealth.HealMultiplier /= _currentRegenMult;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && MaxRegenMult == (other as HealthDependedRegenMultiplier).MaxRegenMult;
    }
}
