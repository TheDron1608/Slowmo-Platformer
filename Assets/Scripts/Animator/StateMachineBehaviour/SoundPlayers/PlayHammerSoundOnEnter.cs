using UnityEngine;

public class PlayHammerSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<HammerBulletReloadingWeapon>().SoundOnHammer.PlaySound(false);
    }
}
