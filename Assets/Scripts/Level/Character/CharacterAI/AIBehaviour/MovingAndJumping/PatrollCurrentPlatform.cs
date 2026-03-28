using System.Collections;
using UnityEngine;

public class PatrollCurrentPlatform : AbstractAIPathfindingMovingAndJumping
{
    public float PatroolSpeedMultiplier = 0.8f;
    public float OnReachedPatformEndAwaitingTime = 1f;
    public bool CanOpenDoors = false;

    private enum PatrollDirection
    {
        LEFT,
        RIGHT,
        NO_MOVE,
        UNSET
    }

    private PatrollDirection _currentPatrollDirection = PatrollDirection.UNSET;
    private PatrollDirection _awaitingPartrollDirection = PatrollDirection.UNSET;
    private Coroutine _currentPatrollDirectionSetCoroutine = null;

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterCollision.OnCollisionChanged += CharacterCollision_OnCollisionChanged;
    }

    private void CharacterCollision_OnCollisionChanged(object sender, CharacterCollision.OnCollisionChangedEventArgs e)
    {
        if (!CanOpenDoors && (e.Collider?.TryGetComponent(out OnInteractToggleOpenDoor door) ?? false))
        {
            _currentPatrollDirectionSetCoroutine = StartCoroutine(SetPatrollDirectionAfterDelay(
                _currentPatrollDirection == PatrollDirection.LEFT ? PatrollDirection.RIGHT : PatrollDirection.LEFT)
                );
        }
    }

    private void OnEnable()
    {
        CharComponents.CharacterMoving.Speed *= PatroolSpeedMultiplier;
        _currentPatrollDirectionSetCoroutine = null;

        if (_awaitingPartrollDirection != PatrollDirection.UNSET)
        {
            _currentPatrollDirection = _awaitingPartrollDirection;
            _awaitingPartrollDirection = PatrollDirection.UNSET;
        }
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
                _currentPatrollDirection = PatrollDirection.NO_MOVE;
            }
            else if (characterTilePosition == new Vector2Int(currentPlatform.Position.x, currentPlatform.Position.y + 1))
            {
                if (_currentPatrollDirectionSetCoroutine == null)
                {
                    _currentPatrollDirectionSetCoroutine = StartCoroutine(SetPatrollDirectionAfterDelay(PatrollDirection.RIGHT));
                }
            }
            else if (characterTilePosition == new Vector2Int(currentPlatform.TailPositionX, currentPlatform.Position.y + 1))
            {
                if (_currentPatrollDirectionSetCoroutine == null)
                {
                    _currentPatrollDirectionSetCoroutine = StartCoroutine(SetPatrollDirectionAfterDelay(PatrollDirection.LEFT));
                }
            }
            else if (_currentPatrollDirection == PatrollDirection.UNSET)
            {
                _currentPatrollDirection = NumberMath.RandomCoinflip() ? PatrollDirection.LEFT : PatrollDirection.RIGHT;
            }

            switch (_currentPatrollDirection)
            {
                case PatrollDirection.LEFT:
                    _selfStateBehaviourAI.Pathfinding.PathTarget = new(new Vector2Int(currentPlatform.Position.x, currentPlatform.Position.y + 1), gameObject);
                    break;
                case PatrollDirection.RIGHT:
                    _selfStateBehaviourAI.Pathfinding.PathTarget = new(new Vector2Int(currentPlatform.TailPositionX, currentPlatform.Position.y + 1), gameObject);
                    break;
                case PatrollDirection.NO_MOVE:
                    _selfStateBehaviourAI.Pathfinding.PathTarget = null;
                    break;
            }
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }

    private IEnumerator SetPatrollDirectionAfterDelay(PatrollDirection value)
    {
        _awaitingPartrollDirection = value;

        yield return new WaitForSeconds(OnReachedPatformEndAwaitingTime);

        _currentPatrollDirection = value;
        _currentPatrollDirectionSetCoroutine = null;
        _awaitingPartrollDirection = PatrollDirection.UNSET;
    }
}
