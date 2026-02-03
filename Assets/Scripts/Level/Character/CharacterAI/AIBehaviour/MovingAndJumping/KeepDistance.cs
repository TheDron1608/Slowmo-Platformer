using UnityEngine;

public class KeepDistance : AbstractAIPathfindingMovingAndJumping
{
    public float MinDistance = 3f;
    public float MaxDistance = 6f;

    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance < MinDistance)
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
                        CharComponents.CharacterMoving.TrySetClumsyAlign(_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.transform.position.x - transform.position.x, true);
                    }
                }
            }

            else if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance > MaxDistance)
            {
                _selfStateBehaviourAI.Pathfinding.PathTarget = new(
                    _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.transform.position,
                    _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.gameObject
                    );
            }

            else
            {
                _selfStateBehaviourAI.Pathfinding.PathTarget = null;
            }
        }

        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}
