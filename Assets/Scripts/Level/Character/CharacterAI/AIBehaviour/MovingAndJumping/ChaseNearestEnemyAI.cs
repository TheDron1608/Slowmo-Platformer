using UnityEngine;

public class ChaseNearestEnemyAI : AbstractAIPathfindingMovingAndJumping
{
    public bool RememberLastEnemyPosition = true;

    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            _selfStateBehaviourAI.AIPathfinding.PathTarget = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.transform.position;
        }
        else if (RememberLastEnemyPosition && _selfStateBehaviourAI.NearestEnemyInfo.LastEnemyPosition.HasValue)
        {
            _selfStateBehaviourAI.AIPathfinding.PathTarget = _selfStateBehaviourAI.NearestEnemyInfo.LastEnemyPosition;
        }
        else
        {
            _selfStateBehaviourAI.AIPathfinding.PathTarget = null;
        }
    }
}
