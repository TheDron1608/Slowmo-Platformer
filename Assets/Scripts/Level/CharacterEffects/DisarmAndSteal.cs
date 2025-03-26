using UnityEngine;

public class DisarmAndSteal : AbstractCharacterEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPartHealth receiverPart)
    {
        if (
            receiverPart.TryGetComponent(out AbstractCharacterComponent disarmedCharacter) &&
            disarmedCharacter.CharComponents.CharacterHolding.CurrentHoldObject != null &&
            sender.TryGetComponent(out AbstractCharacterComponent thiefCharacter)
            )
        {
            disarmedCharacter.CharComponents.CharacterHolding.CurrentHoldObject.Give(thiefCharacter.CharComponents.CharacterHolding);
        }
        RemoveSelf();
    }
}
