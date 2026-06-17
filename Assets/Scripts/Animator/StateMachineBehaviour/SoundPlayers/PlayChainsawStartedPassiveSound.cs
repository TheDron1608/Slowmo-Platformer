using UnityEngine;

public class PlayChainsawStartedPassiveSound : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Chainsaw chainsaw = animator.GetComponent<Chainsaw>();

        if (!chainsaw.PassiveSoundOnStarted.GetIsPlaying() && chainsaw.StartingState == Chainsaw.ChainsawStartState.SUCCESS)
        {
            chainsaw.PassiveSoundOnStarted.PlaySound(true);
        }
    }
}
