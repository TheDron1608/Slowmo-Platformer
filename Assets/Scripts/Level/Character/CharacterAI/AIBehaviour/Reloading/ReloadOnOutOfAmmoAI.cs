using UnityEngine;

public class ReloadOnOutOfAmmoAI : AbstractAIReloading
{
    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
        {
            if (rangedWeapon.GetIsNeedReload())
            {
                CharComponents.CharacterReloading.TryReload();
            }
            if (
                CharComponents.CharacterReloading.GetIsReloading() && 
                rangedWeapon.LoadedLivingAmmoLeft > 0 && 
                _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null
                )
            {
                CharComponents.CharacterReloading.TryFinishReload();
            }
        }
    }
}
