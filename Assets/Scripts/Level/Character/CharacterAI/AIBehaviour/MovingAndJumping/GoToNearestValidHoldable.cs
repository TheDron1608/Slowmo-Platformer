using System.Linq;
using UnityEngine;

public class GoToNearestValidHoldable : AbstractAIPathfindingMovingAndJumping
{
    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null)
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.transform.position;
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}
