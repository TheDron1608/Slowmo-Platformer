using UnityEngine;

public class GibLimb : AbstractCharacterEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPartHealth receiverPart)
    {
        Debug.Log(receiverPart);
        receiverPart.TryGib(sender);
        RemoveSelf();
    }
}
