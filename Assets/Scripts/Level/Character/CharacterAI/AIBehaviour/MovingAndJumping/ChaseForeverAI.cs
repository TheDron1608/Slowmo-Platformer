using UnityEngine;

public class ChaseForeverAI : AbstractAIPathfindingMovingAndJumping
{
    public GameObject ChaseWho;

    protected override void UpdatePathTarget()
    {
        if (ChaseWho != null)
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = new(ChaseWho.transform.position, LayerManager.Instance.GetZLayerOfGameObject(ChaseWho));
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}
