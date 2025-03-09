using Unity.Mathematics;
using UnityEngine;

public class UnloadAmmoOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<RangedWeapon>().OnUnloadFinish();
        animator.GetComponent<RangedWeapon>().OnUnloadFinish();
    }
}
