using UnityEngine;

public class ShieldRaisedOnActiveAnimation : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Shield>().Animator_OnRaisedChanged(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Shield>().Animator_OnRaisedChanged(false);
    }
}
