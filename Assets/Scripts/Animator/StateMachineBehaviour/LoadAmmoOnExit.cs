using UnityEngine;

public class LoadAmmoOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        RangedWeapon weapon = animator.GetComponent<RangedWeapon>();

        int loadAmount = weapon.AmmoAmountPerReload;
        if (!weapon.MagReload)
        {
            loadAmount -= weapon.LoadedLivingAmmoLeft;
        }
        if (loadAmount > weapon.AmmoLeft)
        {
            loadAmount = weapon.AmmoLeft;
        }

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
    }
}
