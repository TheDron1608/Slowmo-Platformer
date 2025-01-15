using UnityEngine;

public class UnloadAmmoOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        RangedWeapon weapon = animator.GetComponent<RangedWeapon>();

        if (weapon.MagReload)
        {
            weapon.LoadedLivingAmmoLeft = 0;
            weapon.LoadedSpentAmmoLeft = 0;
        }
        else
        {
            weapon.LoadedSpentAmmoLeft -= weapon.AmmoAmountPerUnload;
            if (weapon.LoadedSpentAmmoLeft < 0)
            {
                weapon.LoadedSpentAmmoLeft = 0;
            }
        }
    }
}
