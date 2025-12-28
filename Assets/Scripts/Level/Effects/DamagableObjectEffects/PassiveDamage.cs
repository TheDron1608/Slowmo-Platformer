using UnityEngine;

public class PassiveDamage : AbstractDamagableObjectEffect, IMultiplierableEffect
{
    public float DamagePerSecond = 0f;
    public bool AllowOnDead = false;
    public bool AllowOnDying = true;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier 
    { 
        get => _effectMultiplier; 
        set => _effectMultiplier = value; 
    }

    private void FixedUpdate()
    {
        if (
            (AllowOnDead || (!AffectedObject.GetComponent<ObjectEffectsReceiver>()?.GetHasEffect<ILethalEffect>() ?? true)) &&
            (AllowOnDying || (!AffectedObject.GetComponent<ObjectEffectsReceiver>()?.GetHasEffect<ILethalEffect>(true) ?? true))
            )
        {
            AffectedDamagableObject.ApplyDamage(DamagePerSecond * EffectMultiplier * Time.fixedDeltaTime, null, 0f);
        }
    }
}
