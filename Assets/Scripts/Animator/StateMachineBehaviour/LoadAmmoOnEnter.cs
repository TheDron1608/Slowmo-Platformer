using UnityEngine;

public class LoadAmmoOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<RangedWeapon>().OnLoadFinish();
    }
}
