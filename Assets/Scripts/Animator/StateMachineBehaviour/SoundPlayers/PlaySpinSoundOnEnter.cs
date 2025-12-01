using UnityEngine;

public class PlaySpinSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<SpinableMeleeWeapon>().SoundOnSpin.PlaySound();
    }
}
