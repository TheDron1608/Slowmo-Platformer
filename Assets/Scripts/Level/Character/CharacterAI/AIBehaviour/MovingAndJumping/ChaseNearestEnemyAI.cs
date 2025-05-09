using UnityEngine;

public class ChaseNearestEnemyAI : AbstractAIPathfindingMovingAndJumping
{
    public bool RememberLastEnemyPosition = true;

    protected override void UpdatePathTarget()
    {
        if (CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo.NearestEnemy != null)
        {
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.AIPathfinding.PathTarget = CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo.NearestEnemy.transform.position;
        }
        else if (RememberLastEnemyPosition && CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo.LastEnemyPosition.HasValue)
        {
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.AIPathfinding.PathTarget = CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo.LastEnemyPosition;
        }
        else
        {
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.AIPathfinding.PathTarget = null;
        }
    }
}
