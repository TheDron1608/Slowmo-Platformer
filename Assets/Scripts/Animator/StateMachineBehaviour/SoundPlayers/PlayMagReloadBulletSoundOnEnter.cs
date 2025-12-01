using UnityEngine;

public class PlayMagReloadBulletSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<MagReloadingWeapon>().ReloadBulletSound.PlaySound();
    }
}
