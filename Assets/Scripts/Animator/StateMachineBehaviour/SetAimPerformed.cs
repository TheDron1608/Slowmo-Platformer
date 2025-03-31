using UnityEngine;

public class SetAimPerformed : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out AbstractCharacterComponent charComponent)) 
        {
            charComponent.CharComponents.CharacterAiming.AimPerformed = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterAiming.AimPerformed = false;
        }
    }
}
