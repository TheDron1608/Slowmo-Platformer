using UnityEngine;

public class ChaseNearestEnemyAI : AbstractAIPathfindingMovingAndJumping
{
    public bool RememberLastEnemyPosition = true;

    protected override void UpdatePathTarget()
    {
        if (CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemy != null)
        {
            CharComponents.CharacterAIManager.AIPathfinding.PathTarget = CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemy.transform.position;
        }
        else if (RememberLastEnemyPosition && CharComponents.CharacterAIManager.NearestEnemyInfo.LastEnemyPosition.HasValue)
        {
            CharComponents.CharacterAIManager.AIPathfinding.PathTarget = CharComponents.CharacterAIManager.NearestEnemyInfo.LastEnemyPosition;
        }
        else
        {
            CharComponents.CharacterAIManager.AIPathfinding.PathTarget = null;
        }
    }
}
