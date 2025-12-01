using UnityEngine;

public class PlayHolsterSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<HolsterableMeleeWeapon>().SoundOnHolster.PlaySound();
    }
}
