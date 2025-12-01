using UnityEngine;

public class PlayChainsawStartedPassiveSound : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Chainsaw>().PassiveSoundOnStarted.PlaySound(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Chainsaw>().PassiveSoundOnStarted.BreakAllSounds();
    }
}
