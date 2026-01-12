using UnityEngine;

public class LifeStealOnKill : AbstractOnKillEffect, IMultiplierableEffect
{
    public float LifeStealMultiplier = 1f;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnKill(DamagableObject killedObj)
    {
        if (killedObj.TryGetComponent(out AbstractCharacterComponent killedCharacter))
        {
            AffectedDamagableObject.ApplyDamage(-killedCharacter.CharComponents.CharacterHealth.MaxHealth * LifeStealMultiplier * EffectMultiplier, killedCharacter, 0f);
        }
    }
}
