using UnityEngine;

public class ComboDependedDamageMultiplier : AbstractEffect, IDamageMultiplierEffect, IMultiplierableEffect
{
    public float MultiplierPerCombo = 0.1f;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    public float DamageMultiplier
    {
        get => 1f + (ScoreManager.Instance?.CurrentCombo ?? 0f) * MultiplierPerCombo * EffectMultiplier;
    }
}
