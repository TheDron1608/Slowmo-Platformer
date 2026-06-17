using UnityEngine;

public class PlayTryStartChainsawSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Chainsaw chainsaw = animator.GetComponent<Chainsaw>();

        switch (chainsaw.StartingState)
        {
            case Chainsaw.ChainsawStartState.SUCCESS:
                chainsaw.SoundOnSuccessStart.PlaySound();
                break;
            case Chainsaw.ChainsawStartState.FAIL:
                chainsaw.SoundOnTryStart.PlaySound();
                break;
            case Chainsaw.ChainsawStartState.OUT_OF_FUEL:
                chainsaw.SoundOnOutOfFuel.PlaySound();
                break;
        }
    }
}
