using UnityEngine;

public class LoadAmmoOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.gameObject.TryGetComponent(out BulletReloadingWeapon weapon))
        {
            weapon.Unloaded = true;

            int loadAmount = weapon.AmmoAmountPerReload;
            if (loadAmount > 0)
            {
                weapon.AmmoLeft -= loadAmount;
                weapon.LoadedLivingAmmoLeft += loadAmount;
            }
            else if (weapon.LoadedLivingAmmoLeft <= 0)
            {
                weapon.TryUnload();
            }

            if (weapon.LoadedLivingAmmoLeft > weapon.MaxLoadedAmmo)
            {
                weapon.LoadedLivingAmmoLeft = weapon.MaxLoadedAmmo;
            }

            if (
                animator.gameObject.TryGetComponent(out PumpReloadingWeapon pumpReloadingWeapon) &&
                pumpReloadingWeapon.LoadedLivingAmmoLeft >= pumpReloadingWeapon.MaxLoadedAmmo
                )
            {
                pumpReloadingWeapon.FinishReload();
                pumpReloadingWeapon.OutOfAmmo = false;
            }
        }
    }
}
