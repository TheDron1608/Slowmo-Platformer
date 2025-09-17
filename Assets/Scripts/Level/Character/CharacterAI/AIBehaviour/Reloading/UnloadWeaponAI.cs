using UnityEngine;

public class UnloadWeaponAI : AbstractAIReloading
{
    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
        {
            rangedWeapon.TryUnload();
        }
    }
}
