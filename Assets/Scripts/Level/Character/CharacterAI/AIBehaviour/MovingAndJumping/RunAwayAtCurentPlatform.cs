using UnityEngine;

public class RunAwayAtCurentPlatform : AbstractAIPathfindingMovingAndJumping
{
    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            Vector2Int characterTilePosition = TileManager.PositionToTilePosition(transform.position);
            TileManager.NavigationPlatformInfo currentPlatform = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager.GetPlatformUnderPoint(characterTilePosition);

            if (currentPlatform != null)
            {
                Vector2Int targetPosition;
                if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.transform.position.x < transform.position.x)
                {
                    targetPosition = new Vector2Int(currentPlatform.TailPositionX, currentPlatform.Position.y + 1);
                }
                else
                {
                    targetPosition = new Vector2Int(currentPlatform.Position.x, currentPlatform.Position.y + 1);
                }

                if (targetPosition != characterTilePosition)
                {
                    _selfStateBehaviourAI.Pathfinding.PathTarget = new(targetPosition, gameObject);
                }
                else
                {
                    //looks at enemy if is doomed
                    _selfStateBehaviourAI.Pathfinding.PathTarget = null;
                    CharComponents.CharacterMoving.TrySetClumsyAlign(_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.transform.position.x - transform.position.x, true);
                }
            }
        }
    }
}
