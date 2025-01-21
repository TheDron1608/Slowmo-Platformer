using UnityEngine;

public class InvokeOnFinishAttackOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Weapon>().OnFinishAttack();
    }
}
