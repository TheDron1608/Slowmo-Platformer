public class ReloadIfAble : AbstractAIReloading
{
    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
        {
            CharComponents.CharacterReloading.TryReload();
        }
    }
}
