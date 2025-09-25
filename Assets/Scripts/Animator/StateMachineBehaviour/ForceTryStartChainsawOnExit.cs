using UnityEngine;

public class ForceTryStartChainsawOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out Chainsaw chainsaw))
        {
            chainsaw.OnTryStartFinish();
        }
    }
}
