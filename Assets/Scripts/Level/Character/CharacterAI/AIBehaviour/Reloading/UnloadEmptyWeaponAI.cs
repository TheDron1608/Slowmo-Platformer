public class UnloadEmptyWeaponAI : AbstractAIReloading
{
    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
            rangedWeapon.GetIsOutOfAmmo()
            )
        {
            rangedWeapon.TryUnload();
        }
    }
}
