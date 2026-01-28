using System.Collections;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class Gib : AbstractCharacterEffectWithSender, IEntireCharacterEffect, ILethalEffect
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedCharacter.CharacterHolding.ForceDisarm();
        AffectedCharacter.CharacterHealth.Gib(sender);
        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<Gib>();
    }
}
