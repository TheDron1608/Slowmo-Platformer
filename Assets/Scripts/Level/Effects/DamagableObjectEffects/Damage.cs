using UnityEngine;

public class Damage : AbstractDamagableObjectEffectWithSender
{
    public float DamageAmount = 1f;
    public float DamageMultiplierMultiplier = 1f;

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

        AffectedDamagableObject.ApplyDamage(DamageAmount * damageMult * DamageManager.Instance.GlobalDamageMultiplier, sender, DamageMultiplierMultiplier);

        RemoveSelf();
    }
}
