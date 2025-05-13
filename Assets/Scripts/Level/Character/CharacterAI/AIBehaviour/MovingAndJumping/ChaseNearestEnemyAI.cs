using UnityEngine;

public class ChaseNearestEnemyAI : AbstractAIPathfindingMovingAndJumping
{
    public bool RememberLastEnemyPosition = true;

    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = new(_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.transform.position, gameObject);
        }
        else if (RememberLastEnemyPosition && _selfStateBehaviourAI.NearestEnemyInfo.LastEnemyPosition.HasValue)
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = new(_selfStateBehaviourAI.NearestEnemyInfo.LastEnemyPosition.Value, gameObject);
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}
