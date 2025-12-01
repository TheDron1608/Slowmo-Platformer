using UnityEngine;

public class PlayLoadBulletSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<BulletReloadingWeapon>().SoundOnLoadBullet.PlaySound();
    }
}
