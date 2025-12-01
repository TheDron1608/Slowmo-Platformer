using UnityEngine;

public class PlayLoadSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<RangedWeapon>().SoundOnLoad.PlaySound();
    }
}
