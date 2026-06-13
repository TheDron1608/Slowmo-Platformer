using UnityEngine;

public class PlayMagReloadBulletSoundOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        MagReloadingWeapon magWeapon = animator.GetComponent<MagReloadingWeapon>();
        if (magWeapon.LoadedLivingAmmoLeft + magWeapon.LoadedSpentAmmoLeft > 1)
        {
            magWeapon.ReloadBulletSound.PlaySound();
        }
        else
        {
            magWeapon.UnloadBulletSound.PlaySound();
        }
    }
}
