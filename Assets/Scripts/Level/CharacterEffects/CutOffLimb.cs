using UnityEngine;

public class CutOffLimb : AbstractCharacterEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (receiverPart.TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.CharPartHealth.TryCutOff(sender);
        }
        else
        {
            throw new UnityException("Trying cut off " + receiverPart.name + ", this character part must contain CharacterLimbPart component for this");
        }

        RemoveSelf();
    }
}
