using UnityEngine;

public class TryInvokeOnFinishHammerringOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var hammerWeapon = animator.GetComponent<HammerBulletReloadingWeapon>();

        if (hammerWeapon.IsHammerring)
        {
            hammerWeapon.OnFinishHammerring();
        }
    }
}
