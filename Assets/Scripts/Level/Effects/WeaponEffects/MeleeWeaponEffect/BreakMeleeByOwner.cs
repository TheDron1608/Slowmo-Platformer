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
        else if (MeleeWeapon.TryGetComponent(out Chainsaw chainsawWeapon))
        {
            chainsawWeapon.FuelLeft = 0f;
        }
        else
        {
            Destroy(MeleeWeapon.gameObject);
        }

        RemoveSelf();
    }
}
