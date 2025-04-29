using System.Linq;
using UnityEngine;

public abstract class AbstractAIPathfindingMovingAndJumping : AbstractAIMovingAndJumping
{
    private AbstractAIPathfinding.PathChainElement _currentChain = null;

    private void FixedUpdate()
    {
        UpdatePathTarget();
        UpdateActionsToReachPathTarget();
    }

    protected abstract void UpdatePathTarget();

    private void UpdateActionsToReachPathTarget()
    {
        Vector2Int characterTilePosition = TileManager.PositionToTilePosition(CharComponents.transform.position);

        if (!CharComponents.CharacterAIManager.AIPathfinding.PathChain.Contains(_currentChain))
        {
            _currentChain = CharComponents.CharacterAIManager.AIPathfinding.PathChain.FirstOrDefault();
            while (_currentChain.PrevElement != null)
            {
                _currentChain = _currentChain.PrevElement;
            }
        }
        if(_currentChain != null && _currentChain.TargetPosition == characterTilePosition)
        {
            _currentChain = _currentChain.NextElement;
        }
        if (_currentChain == null)
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
            CharComponents.CharacterJumping.StopJump();
            return;
        }

        _currentChain.Debug_DrawChain(Color.red, Time.fixedDeltaTime);

        //moving to target
        if (characterTilePosition.x > _currentChain.TargetPosition.x)
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Left);
        }
        else if (characterTilePosition.x < _currentChain.TargetPosition.x)
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Right);
        }
        else
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
        }

        //jumping to target
        if (characterTilePosition.y < _currentChain.TargetPosition.y && !CharComponents.CharacterJumping.GetIsJumping())
        {
            CharComponents.CharacterJumping.TryStartJump();
        }
    }
}
