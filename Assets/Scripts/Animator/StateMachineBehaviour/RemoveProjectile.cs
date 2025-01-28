using UnityEngine;

public class RemoveProjectile : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<AbstractProjectile>().RemoveSelf();
    }
}
