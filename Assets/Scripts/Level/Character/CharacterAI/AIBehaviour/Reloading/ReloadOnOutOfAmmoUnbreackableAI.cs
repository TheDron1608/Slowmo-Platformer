public class ReloadOnOutOfAmmoUnbreackableAI : AbstractAIReloading
{
    private void FixedUpdate()
    {
        if (
            !CharComponents.CharacterReloading.GetIsReloading() &&
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
        {
            if (rangedWeapon.GetIsNeedReload())
            {
                CharComponents.CharacterReloading.TryReload();
            }
        }
    }
}
