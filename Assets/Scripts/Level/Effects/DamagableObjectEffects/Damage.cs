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
        AffectedDamagableObject.ApplyDamage(DamageAmount * DamageManager.Instance.GlobalDamageMultiplier, sender, DamageMultiplierMultiplier);

        RemoveSelf();
    }
}
