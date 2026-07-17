using UnityEngine;

public class ShakeHoldDistanceWhenIsHolded : StateMachineBehaviour
{
    public float ChangeDistanceRange = 0.5f;

    private float _previousShakeDistance = 0f;
    private Holdable _holdableComponent;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _holdableComponent = animator.GetComponent<Holdable>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _holdableComponent.ExtraHoldDistance -= _previousShakeDistance;

        float newShakeDistance = Random.value * ChangeDistanceRange;

        _holdableComponent.ExtraHoldDistance += newShakeDistance;

        _previousShakeDistance = newShakeDistance;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _holdableComponent.ExtraHoldDistance -= _previousShakeDistance;
        _previousShakeDistance = 0f;
    }
}
