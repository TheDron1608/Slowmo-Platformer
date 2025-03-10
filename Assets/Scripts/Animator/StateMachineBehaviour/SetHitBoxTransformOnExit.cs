using Unity.Mathematics;
using UnityEngine;

public class SetHitBoxTransformOnExit : StateMachineBehaviour
{
    public CharacterHitbox.AvaibleHitBoxTransforms HitBoxOnExit;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterPartsManager.SetHitBoxTransform(HitBoxOnExit);
    }
}
