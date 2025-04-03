using UnityEngine;

public class RemoveAllStuckedObjectsOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.TryGetComponent(out AbstractCharacterComponent charComponent)) 
        {
            charComponent.CharComponents.CharacterStuckedObjects.RemoveAllStuckedObjects();
        }
    }
}
