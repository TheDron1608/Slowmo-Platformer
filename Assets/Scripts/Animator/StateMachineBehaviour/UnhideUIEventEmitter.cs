using System;
using UnityEngine;

public class UnhideUIEventEmitter : StateMachineBehaviour
{
    public static event EventHandler UnhideUIEventCalled;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UnhideUIEventCalled?.Invoke(this, EventArgs.Empty);
    }
}
