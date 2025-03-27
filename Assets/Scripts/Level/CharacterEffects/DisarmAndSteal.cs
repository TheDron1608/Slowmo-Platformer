using UnityEngine;

public class DisarmAndSteal : AbstractCharacterEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPartHealth receiverPart)
    {
        if (
            receiverPart.TryGetComponent(out AbstractCharacterComponent disarmedCharacter) &&
            disarmedCharacter.CharComponents.CharacterHolding.CurrentHoldObject != null &&
            sender.GetComponent<AbstractProjectile>().Weapon.TryGetComponent(out UnarmedWeapon thiefUnarmedAttack)
            )
        {
            disarmedCharacter.CharComponents.CharacterHolding.CurrentHoldObject.Give(thiefUnarmedAttack.CharComponents.CharacterHolding);
        }
        RemoveSelf();
    }
}
