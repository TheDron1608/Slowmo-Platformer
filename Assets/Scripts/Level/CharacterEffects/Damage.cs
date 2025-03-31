using UnityEngine;

public class Damage : AbstractCharacterEffectWithSender
{
    public float DamageAmount = 1f;

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (receiverPart.TryGetComponent(out CharacterLimbPart limbPart))
        {
            if (sender.TryGetComponent(out AbstractProjectile projectile))
            {
                limbPart.CharPartHealth.ApplyDamage(DamageAmount, projectile);
            }
            else
            {
                limbPart.CharPartHealth.ApplyDamage(DamageAmount, sender);
            }
        }
        else
        {
            throw new UnityException("Trying cut off " + receiverPart.name + ", this character part must contain CharacterLimbPart component for this");
        }

        RemoveSelf();
    }
}
