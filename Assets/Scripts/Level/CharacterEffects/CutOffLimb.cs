using UnityEngine;

public class CutOffLimb : AbstractCharacterEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPartHealth receiverPart)
    {
        receiverPart.TryCutOff(sender);
        RemoveSelf();
    }
}
