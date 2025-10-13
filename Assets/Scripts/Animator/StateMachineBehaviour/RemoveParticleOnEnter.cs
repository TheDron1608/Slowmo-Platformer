using UnityEngine;

public class RemoveParticleOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AbstractParticle>().RemoveParticle();
    }
}
