using UnityEngine;

public class BusyAnimationAllowMovement : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out AbstractCharacterComponent charComponent)) 
        {
            charComponent.CharComponents.CharacterVisual.AllowMovementOnBusyAnimation = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterVisual.AllowMovementOnBusyAnimation = false;
        }
    }
}
