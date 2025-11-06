using UnityEngine;

public class ForceStopRollingOnExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<CharacterRolling>().ForceStopRolling();
    }
}
