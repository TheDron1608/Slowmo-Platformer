using UnityEngine;

public class ReloadBulletOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<MagReloadingWeapon>().OnReloadBulletFinish();
    }
}
