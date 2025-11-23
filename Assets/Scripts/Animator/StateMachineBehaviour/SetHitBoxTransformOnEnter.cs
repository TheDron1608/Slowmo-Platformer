using UnityEngine;

public class SetHitBoxTransformOnEnter : StateMachineBehaviour
{
    public CharacterHitbox.AvaibleHitBoxTransforms HitBoxOnEnter;
    public float ChangeDuration = .25f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterPartsManager.SetHitBoxTransform(HitBoxOnEnter, ChangeDuration);
    }
}
