using System.Linq;
using UnityEngine;

public class GoToNearestValidHoldable : AbstractAIPathfindingMovingAndJumping
{
    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.NearestPrefferedHoldable.NearestPrefferedHoldable != null)
        {
            _selfStateBehaviourAI.AIPathfinding.PathTarget = _selfStateBehaviourAI.NearestPrefferedHoldable.NearestPrefferedHoldable.transform.position;
        }
        else
        {
            _selfStateBehaviourAI.AIPathfinding.PathTarget = null;
        }
    }
}
