using UnityEngine;

public class Damage : AbstractEffectWithSender
{
    public float DamageAmount = 1f;

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedObject.TryGetComponent(out CharacterPart charPart))
        {
            charPart.CharComponents.CharacterHealth.ApplyDamage(DamageAmount, sender, charPart);
        }
        else
        {
            AffectedObject.GetComponent<DamagableObject>()?.ApplyDamage(DamageAmount, sender);
        }

        RemoveSelf();
    }
}
