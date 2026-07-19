using UnityEngine;

public class LoadAmmoAllOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        BulletReloadingWeapon brw = animator.GetComponent<BulletReloadingWeapon>();
        int oldLoadAmount = brw.AmmoAmountPerReload;

        brw.AmmoAmountPerReload = brw.MaxLoadedAmmo;
        brw.OnLoadFinish();
        brw.AmmoAmountPerReload = oldLoadAmount;
    }
}
