using UnityEngine;

public class DisarmAndSteal : AbstractCharacterEffectWithSender, IEntireCharacterEffect
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (
            AffectedCharacter.CharacterHolding.CurrentHoldObject != null &&
            sender.GetComponent<AbstractProjectile>().Weapon.TryGetComponent(out UnarmedWeapon thiefUnarmedAttack)
            )
        {
            AffectedCharacter.CharacterHolding.CurrentHoldObject.Give(thiefUnarmedAttack.CharComponents.CharacterHolding);
        }
        RemoveSelf();
    }
}
