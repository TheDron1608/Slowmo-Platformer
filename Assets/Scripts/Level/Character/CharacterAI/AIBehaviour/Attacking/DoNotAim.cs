using UnityEngine;

public class DoNotAim : AbstractAIAttacking
{
    private void FixedUpdate()
    {
        CharComponents.CharacterAiming.AimWeaponDown = true;
    }

    private void OnDisable()
    {
        CharComponents.CharacterAiming.AimWeaponDown = false;
    }
}
