using UnityEngine;

public class DisarmAndSteal : AbstractCharacterEffectWithSender, IEntireCharacterEffect
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (
            AffectedCharacter.CharacterHolding.CurrentHoldObject != null &&
            sender.GetComponent<AbstractProjectile>()?.Owner != null
            )
        {
            AffectedCharacter.CharacterHolding.ForceDisarm(sender.GetComponent<AbstractProjectile>()?.Owner);
        }
        RemoveSelf();
    }
}
