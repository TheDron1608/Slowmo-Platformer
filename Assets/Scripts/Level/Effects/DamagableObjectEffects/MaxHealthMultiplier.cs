using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class MaxHealthMultiplier : AbstractDamagableObjectEffectWithSender, IMultiplierableEffect
{
    public float HealthMultplier = 1f;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedDamagableObject.ApplyMaxHealth(AffectedDamagableObject.MaxHealth * Mathf.LerpUnclamped(1f, HealthMultplier, EffectMultiplier), sender);
    }
}
