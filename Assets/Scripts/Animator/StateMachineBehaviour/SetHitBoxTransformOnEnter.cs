using Unity.Mathematics;
using UnityEngine;

public class SetHitBoxTransformOnEnter : StateMachineBehaviour
{
    public CharacterHitbox.AvaibleHitBoxTransforms HitBoxOnEnter;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterPartsManager.SetHitBoxTransform(HitBoxOnEnter);
    }
}
