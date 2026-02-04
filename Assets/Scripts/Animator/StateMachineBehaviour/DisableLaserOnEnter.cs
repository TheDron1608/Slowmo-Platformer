using Unity.Mathematics;
using UnityEngine;

public class DisableLaserOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<LaserHoldable>().LaserEnabled = false;
    }
}
