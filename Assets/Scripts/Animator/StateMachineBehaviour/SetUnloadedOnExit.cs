using UnityEngine;

public class SetUnloadedOnExit : StateMachineBehaviour
{
    public bool Value;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.Unloaded = Value;
        }
    }
}
