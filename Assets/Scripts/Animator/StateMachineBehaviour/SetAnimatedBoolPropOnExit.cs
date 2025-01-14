using UnityEngine;

public class SetAnimatedBoolPropOnExit : StateMachineBehaviour
{
    public string SetPropName;
    public bool Value;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(SetPropName, Value);
    }
}
