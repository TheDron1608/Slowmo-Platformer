using UnityEngine;

public class PassiveDamage : AbstractDamagableObjectEffect, IMultiplierableEffect
{
    public float DamagePerSecond = 0f;
    public bool AllowOnDead = false;
    public bool AllowOnDying = true;

    private float _effectMultiplier = 1f;
    private ObjectEffectsReceiver _affectedObjectEffectsReceiver = null;

    protected override void OnApply()
    {
        base.OnApply();

        AffectedObject.TryGetComponent(out _affectedObjectEffectsReceiver);
    }

    public float EffectMultiplier 
    { 
        get => _effectMultiplier; 
        set => _effectMultiplier = value; 
    }

    private void FixedUpdate()
    {
        if (
            (AllowOnDead || (!_affectedObjectEffectsReceiver?.GetHasEffect<ILethalEffect>() ?? true)) &&
            (AllowOnDying || (!_affectedObjectEffectsReceiver?.GetHasEffect<ILethalEffect>(true) ?? true))
            )
        {
            AffectedDamagableObject.ApplyDamage(DamagePerSecond * EffectMultiplier * Time.fixedDeltaTime, null, 0f);
        }
    }
}
