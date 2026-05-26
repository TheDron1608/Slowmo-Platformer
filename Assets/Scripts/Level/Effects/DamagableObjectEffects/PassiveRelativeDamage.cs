using Unity.Mathematics;
using UnityEngine;

public class PassiveRelativeDamage : AbstractDamagableObjectEffect, IMultiplierableEffect
{
    public float DamageFromMaxHealthAmountPerSecond = 0f;
    public float MinDamage = 0.5f;
    public bool AllowOnDead = false;
    public bool AllowOnDying = true;

    private float _effectMultiplier = 1f;
    private ObjectEffectsReceiver _affectedObjectEffectsReceiver = null;

    public float EffectMultiplier 
    { 
        get => _effectMultiplier; 
        set => _effectMultiplier = value; 
    }

    protected override void OnApply()
    {
        base.OnApply();

        AffectedObject.TryGetComponent(out _affectedObjectEffectsReceiver);
    }

    private void FixedUpdate()
    {
        if (
            (AllowOnDead || (!_affectedObjectEffectsReceiver?.GetHasEffect<ILethalEffect>() ?? true)) &&
            (AllowOnDying || (!_affectedObjectEffectsReceiver?.GetHasEffect<ILethalEffect>(true) ?? true))
            )
        {
            AffectedDamagableObject.ApplyDamage( 
                math.max(DamageFromMaxHealthAmountPerSecond * AffectedDamagableObject.MaxHealth, MinDamage) * EffectMultiplier * Time.fixedDeltaTime, 
                null, 
                0f
                );
        }
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            DamageFromMaxHealthAmountPerSecond == (other as PassiveRelativeDamage).DamageFromMaxHealthAmountPerSecond &&
            AllowOnDead == (other as PassiveRelativeDamage).AllowOnDead &&
            AllowOnDying == (other as PassiveRelativeDamage).AllowOnDying;
    }
}
