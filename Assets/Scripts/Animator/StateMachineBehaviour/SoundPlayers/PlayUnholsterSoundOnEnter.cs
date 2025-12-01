using UnityEngine;

public class PlayUnholsterSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<HolsterableMeleeWeapon>().SoundOnUnholster.PlaySound();
    }
}
