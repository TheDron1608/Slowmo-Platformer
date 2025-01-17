using UnityEngine;

public class LoadAmmoOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<RangedWeapon>().OnLoadFinish();
    }
}
