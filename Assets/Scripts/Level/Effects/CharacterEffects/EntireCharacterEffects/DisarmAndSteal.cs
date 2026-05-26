using UnityEngine;

public class DisarmAndSteal : AbstractCharacterEffectWithSender, IEntireCharacterEffect
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (
            AffectedCharacter.CharacterHolding.CurrentHoldObject != null &&
            sender.TryGetComponent(out AbstractProjectile projectile) &&
            projectile.Owner != null
            )
        {
            AffectedCharacter.CharacterHolding.ForceDisarm(projectile.Owner);
        }
        RemoveSelf();
    }
}
