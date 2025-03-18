using UnityEngine;

public class RemoveAllStuckedObjectsOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.transform.parent.TryGetComponent(out AbstractCharacterComponent charComponent)) 
        {
            charComponent.CharComponents.CharacterStuckedObjects.RemoveAllStuckedObjects();
        }
    }
}
