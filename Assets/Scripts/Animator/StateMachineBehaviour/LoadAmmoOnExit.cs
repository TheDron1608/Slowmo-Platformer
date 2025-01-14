using UnityEngine;

public class LoadAmmoOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        RangedWeapon weapon = animator.GetComponent<RangedWeapon>();

        if (weapon.AmmoLeft >= weapon.AmmoAmountPerReload)
        {
            weapon.LoadedLivingAmmoLeft += weapon.AmmoAmountPerReload;
            weapon.AmmoLeft -= weapon.AmmoAmountPerReload;
        }
        else if (weapon.AmmoLeft > 0)
        {
            weapon.LoadedLivingAmmoLeft = weapon.AmmoAmountPerReload;
            weapon.AmmoLeft = 0;
        }
        else
        {
            weapon.TryUnload();
        }

        if (weapon.LoadedLivingAmmoLeft > weapon.MaxLoadedAmmo)
        {
            weapon.LoadedLivingAmmoLeft = weapon.MaxLoadedAmmo;
        }
    }
}
