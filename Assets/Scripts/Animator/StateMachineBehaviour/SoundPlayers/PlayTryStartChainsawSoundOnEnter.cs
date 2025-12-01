using UnityEngine;

public class PlayTryStartChainsawSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Chainsaw>().SoundOnTryStart.PlaySound();
    }
}
