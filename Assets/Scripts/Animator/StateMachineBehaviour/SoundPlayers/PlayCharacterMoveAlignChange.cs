using UnityEngine;

public class PlayCharacterMoveAlignChangeSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterMoving.MoveAlignChangeSound.PlaySound();
    }
}
