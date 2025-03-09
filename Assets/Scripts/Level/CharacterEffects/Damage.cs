using UnityEngine;

public class Damage : AbstractCharacterEffectWithSender
{
    public float DamageAmount = 1f;

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPartHealth receiverPart)
    {
        if (sender.TryGetComponent(out AbstractProjectile projectile))
        {
            receiverPart.ApplyDamage(DamageAmount, projectile);
        }

        RemoveSelf();
    }
}
