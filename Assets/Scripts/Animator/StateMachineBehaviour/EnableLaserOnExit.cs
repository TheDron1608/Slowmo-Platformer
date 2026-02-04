using Unity.Mathematics;
using UnityEngine;

public class EnableLaserOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<LaserHoldable>().LaserEnabled = true;
    }
}
