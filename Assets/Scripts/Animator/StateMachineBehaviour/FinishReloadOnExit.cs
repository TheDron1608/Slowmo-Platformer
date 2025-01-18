using UnityEngine;

public class FinishReloadOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<RangedWeapon>().TryFinishReload();
    }
}
