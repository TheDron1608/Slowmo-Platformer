using UnityEngine;

public class LoadBulletReloadingWeaponOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<BulletReloadingWeapon>().OnLoadFinishNoAmmo();
    }
}
