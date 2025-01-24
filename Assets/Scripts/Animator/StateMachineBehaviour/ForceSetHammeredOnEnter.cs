using UnityEngine;

public class ForceSetHammeredOnEnter : StateMachineBehaviour
{
    public bool Value;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon))
        {
            hammerWeapon.ForceSetHammered(Value);
        }
    }
}
