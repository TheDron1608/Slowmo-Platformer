using UnityEngine;

public class PlayCharacterRollSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterRolling.SoundOnRoll.PlaySound();
    }
}
