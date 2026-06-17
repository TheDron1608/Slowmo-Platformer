using UnityEngine;

public class StopChainsawStartedPassiveSound : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Chainsaw>().PassiveSoundOnStarted.BreakAllSounds();
    }
}
