using Unity.Mathematics;
using UnityEngine;

public class SetHitBoxTransform : StateMachineBehaviour
{
    public CharacterHitbox.AvaibleHitBoxTransforms? HitBoxOnEnter = null;
    public CharacterHitbox.AvaibleHitBoxTransforms? HitBoxOnExit = null;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (HitBoxOnEnter.HasValue)
        {
            animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterRigidBodyCapsuleColliderHitBox.SetHitBoxTransform(HitBoxOnEnter.Value);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (HitBoxOnExit.HasValue)
        {
            animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterRigidBodyCapsuleColliderHitBox.SetHitBoxTransform(HitBoxOnExit.Value);
        }
    }
}
