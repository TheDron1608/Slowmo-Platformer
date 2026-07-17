using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class RegenDependedHealthUp : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float HealthPerRegen = 0.25f;

    private float _oldHealtUp = 0f;

    private void FixedUpdate()
    {
        float newHealthUp = (AffectedCharacter.CharacterHealth.HealMultiplier - 1f) / HealthPerRegen;
        AffectedCharacter.CharacterHealth.ApplyMaxHealth(AffectedCharacter.CharacterHealth.MaxHealth - _oldHealtUp + newHealthUp, null);
        _oldHealtUp = newHealthUp;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterHealth.ApplyMaxHealth(AffectedCharacter.CharacterHealth.MaxHealth - _oldHealtUp, null);
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && HealthPerRegen == (other as RegenDependedHealthUp).HealthPerRegen;
    }
}
