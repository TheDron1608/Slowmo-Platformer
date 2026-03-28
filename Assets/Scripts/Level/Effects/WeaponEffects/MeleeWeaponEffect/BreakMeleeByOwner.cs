using Unity.VisualScripting;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class BreakMeleeByOwner : AbstractMeleeWeaponEffectWithSender
{
    public bool IncludeDestroyBrokenWeapon = true;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (MeleeWeapon.TryGetComponent(out BreakableHoldable breakableWeapon))
        {
            if (IncludeDestroyBrokenWeapon)
            {
                breakableWeapon.SpawnObjectsOnBreak.RemoveAll(e => e.GetComponent<Weapon>() != null);
            }
            breakableWeapon.BreakObject(sender?.GetComponent<AbstractCharacterComponent>()?.CharComponents.CharacterHolding);
        }
        else
        {
            Destroy(MeleeWeapon.gameObject);
        }

        RemoveSelf();
    }
}
