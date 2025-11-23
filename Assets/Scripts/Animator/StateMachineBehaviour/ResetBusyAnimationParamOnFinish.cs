using UnityEngine;

public class ResetBusyAnimationParamOnFinish : StateMachineBehaviour
{
    /// <summary>
    /// resets only if current state equals to ResetState value
    /// </summary>
    public CharacterVisual.CharacterPartBusyStates ResetState;

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.transform.TryGetComponent(out CharacterVisual charVisual))
        {
            if (ResetState == charVisual.CurrentBusyAnimation)
            {
                charVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.NONE;
            }
        }
        else
        {
            throw new UnityException("CharacterVisual component not found in " + animator.transform.parent.gameObject.name);
        }
    }
}
