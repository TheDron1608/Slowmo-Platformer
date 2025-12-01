using UnityEngine;

public class PlayHammerSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<HammerBulletReloadingWeapon>().SoundOnHammer.PlaySound(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<HammerBulletReloadingWeapon>().SoundOnHammer.BreakAllSounds();
    }
}
