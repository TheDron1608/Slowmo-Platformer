using UnityEngine;

public class ChaseForeverAI : AbstractAIPathfindingMovingAndJumping
{
    public GameObject ChaseWho;

    protected override void UpdatePathTarget()
    {
        if (ChaseWho != null)
        {
            _selfStateBehaviourAI.AIPathfinding.PathTarget = ChaseWho.transform.position;
        }
        else
        {
            _selfStateBehaviourAI.AIPathfinding.PathTarget = null;
        }
    }
}
