
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class RelativeDamage : AbstractDamagableObjectEffectWithSender
{
    const float MIN_DAMAGE = 0.005f;

    public float DamagePerCurrentHealth = 1f;
    public bool IncludeDamageMult = true;

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        float damageMult = 1f;
        if (sender != null && sender.TryGetComponent(out ObjectEffectsReceiver effectsReceiver))
        {
            foreach (IDamageMultiplierEffect damageMultiplierEffect in effectsReceiver.GetEffects<IDamageMultiplierEffect>())
            {
                damageMult *= damageMultiplierEffect.DamageMultiplier;
            }
        }

        float targetDamage = AffectedDamagableObject.CurrentHealth * DamagePerCurrentHealth * damageMult * (DamageManager.Instance?.GlobalDamageMultiplier ?? 1f);
        if (targetDamage > MIN_DAMAGE)
        {
            AffectedDamagableObject.ApplyDamage(targetDamage, sender, IncludeDamageMult);
        }

        RemoveSelf();
    }
}