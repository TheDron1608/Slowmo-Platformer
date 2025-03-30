using UnityEngine;
using UnityEngine.Animations;

public class ResetBusyAnimationParamOnFinish : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.transform.parent.parent.TryGetComponent(out CharacterVisual charVisual))
        {
            charVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.NONE;
        }
        else
        {
            throw new UnityException("CharacterVisual component not found in " + animator.transform.parent.gameObject.name);
        }
    }
}
