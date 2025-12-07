using UnityEngine;

public class PlayCharacterJumpSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterJumping.SoundOnJump.PlaySound();
    }
}
