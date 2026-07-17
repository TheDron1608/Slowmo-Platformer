using UnityEngine;

public class ChangeHoldDistanceWhenIsHolded : StateMachineBehaviour
{
    public float ChangeDistanceRange = 0.5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out Holdable holdable))
        {
            holdable.ExtraHoldDistance += ChangeDistanceRange;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out Holdable holdable))
        {
            holdable.ExtraHoldDistance -= ChangeDistanceRange;
        }
    }
}
