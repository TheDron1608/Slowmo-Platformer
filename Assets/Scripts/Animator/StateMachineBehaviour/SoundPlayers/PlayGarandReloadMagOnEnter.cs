using UnityEngine;

public class PlayGarandReloadMagOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Garand>().SoundOnLoadMag.PlaySound();
    }
}
