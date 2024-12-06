using UnityEngine;

public class OneFrameState : StateMachineBehaviour
{

    [SerializeField]
    private float startNormalizedTime;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > startNormalizedTime -  0.05f && stateInfo.normalizedTime < startNormalizedTime + 0.05f)
        {
            animator.Play(stateInfo.shortNameHash, layerIndex, startNormalizedTime);
        }
    }
}
