using UnityEngine;

public class PatroolCurrentPlatform : AbstractAIPathfindingMovingAndJumping
{
    public float PatroolSpeedMultiplier = 0.8f;

    private enum PatroolDirection
    {
        LEFT,
        RIGHT,
        NO_MOVE,
        PICK_RANDOM
    }

    private PatroolDirection _currentPatroolDirection = PatroolDirection.PICK_RANDOM;

    private void OnEnable()
    {
        _currentPatroolDirection = PatroolDirection.PICK_RANDOM;
        CharComponents.CharacterMoving.Speed *= PatroolSpeedMultiplier;
    }

    private void OnDisable()
    {
        CharComponents.CharacterMoving.Speed /= PatroolSpeedMultiplier;
    }

    protected override void UpdatePathTarget()
    {
        Vector2Int characterTilePosition = TileManager.PositionToTilePosition(transform.position);
        var currentPlatform = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager.GetPlatformUnderPoint(characterTilePosition);

        if (currentPlatform != null)
        {
            if (currentPlatform.Width == 1)
            {
                _currentPatroolDirection = PatroolDirection.NO_MOVE;
            }
            else if (characterTilePosition == new Vector2Int(currentPlatform.Position.x, currentPlatform.Position.y + 1))
            {
                _currentPatroolDirection = PatroolDirection.RIGHT;
            }
            else if (characterTilePosition == new Vector2Int(currentPlatform.TailPositionX, currentPlatform.Position.y + 1))
            {
                _currentPatroolDirection = PatroolDirection.LEFT;
            }
            else if (_currentPatroolDirection == PatroolDirection.PICK_RANDOM)
            {
                _currentPatroolDirection = Random.value > 0.5f ? PatroolDirection.LEFT : PatroolDirection.RIGHT;
            }

            switch (_currentPatroolDirection)
            {
                case PatroolDirection.LEFT:
                    _selfStateBehaviourAI.Pathfinding.PathTarget = new Vector2(currentPlatform.Position.x, currentPlatform.Position.y + 1);
                    break;
                case PatroolDirection.RIGHT:
                    _selfStateBehaviourAI.Pathfinding.PathTarget = new Vector2(currentPlatform.TailPositionX, currentPlatform.Position.y + 1);
                    break;
                case PatroolDirection.NO_MOVE:
                    _selfStateBehaviourAI.Pathfinding.PathTarget = null;
                    break;
            }
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}
