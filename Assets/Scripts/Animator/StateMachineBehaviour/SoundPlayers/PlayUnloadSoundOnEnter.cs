using UnityEngine;

public class PlayUnloadSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<RangedWeapon>().SoundOnUnload.PlaySound();
    }
}
