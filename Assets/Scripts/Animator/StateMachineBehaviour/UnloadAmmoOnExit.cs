using Unity.Mathematics;
using UnityEngine;

public class UnloadAmmoOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.gameObject.TryGetComponent(out BulletReloadingWeapon weapon))
        {
            weapon.Unloaded = true;
            {
                animator.GetComponent<RangedWeapon>().SpawnBulletParticles(math.min(weapon.AmmoAmountPerUnload, weapon.LoadedSpentAmmoLeft));
                weapon.LoadedSpentAmmoLeft -= weapon.AmmoAmountPerUnload;
                if (weapon.LoadedSpentAmmoLeft < 0)
                {
                    weapon.LoadedSpentAmmoLeft = 0;
                }
            }

            if (animator.gameObject.TryGetComponent(out PumpReloadingWeapon pumpWeapon))
            {
                if (pumpWeapon.AmmoLeft <= 0 && pumpWeapon.LoadedLivingAmmoLeft <= 0)
                {
                    pumpWeapon.OutOfAmmo = true;
                }
            }
        }
    }
}
