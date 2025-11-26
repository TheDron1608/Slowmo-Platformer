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
}
