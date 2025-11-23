using System;
using UnityEngine;

public class ReselectFloorEventEmitter : StateMachineBehaviour
{
    public static event EventHandler<int> ReselectFloorEventCalled;

    [SerializeField]
    private int _floor;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ReselectFloorEventCalled?.Invoke(this, _floor);
    }
}
