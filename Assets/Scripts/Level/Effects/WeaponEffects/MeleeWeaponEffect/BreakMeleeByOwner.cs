using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class BreakMeleeByOwner : AbstractMeleeWeaponEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (MeleeWeapon.TryGetComponent(out BreakableHoldable breakableWeapon))
        {
            breakableWeapon.BreakObject(sender?.GetComponent<AbstractCharacterComponent>()?.CharComponents.CharacterHolding);
        }
        else
        {
            Destroy(MeleeWeapon.gameObject);
        }

        RemoveSelf();
    }
}
