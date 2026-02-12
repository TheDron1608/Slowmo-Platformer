using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class BreakShieldByOwner : AbstractShieldEffectWithSender
{
    public float ShieldDamage = 2.5f;
    public float ShieldDamageDelay = .25f;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        StartCoroutine(DamageShieldCoroutine());
    }

    private IEnumerator DamageShieldCoroutine()
    {
        while (Shield != null && !Shield.IsDestroyed())
        {
            Shield.ApplyDamage(ShieldDamage, Sender);

            if (Shield != null && !Shield.IsDestroyed())
            {
                yield return new WaitForSeconds(ShieldDamageDelay);
            }
        }

        RemoveSelf();
    }
}
