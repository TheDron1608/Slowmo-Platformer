using UnityEngine;

public class GibLimb : AbstractCharacterEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPartHealth receiverPart)
    {
        receiverPart.TryGib(sender);
        RemoveSelf();
    }
}
