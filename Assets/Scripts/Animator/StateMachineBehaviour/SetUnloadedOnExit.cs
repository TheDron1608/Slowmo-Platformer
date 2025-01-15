using UnityEngine;

public class SetUnloadedOnExit : StateMachineBehaviour
{
    public bool Value;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<RangedWeapon>().Unloaded = Value;
    }
}
