using UnityEngine;

public class GibLimb : AbstractCharacterEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (receiverPart.TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.CharPartHealth.TryGib(sender);
        }
        else
        {
            throw new UnityException("Trying cut off " + receiverPart.name + ", this character part must contain CharacterLimbPart component for this");
        }

        RemoveSelf();
    }
}
