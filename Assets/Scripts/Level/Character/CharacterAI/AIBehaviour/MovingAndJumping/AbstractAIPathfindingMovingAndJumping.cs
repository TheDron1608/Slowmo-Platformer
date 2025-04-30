using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractAIPathfindingMovingAndJumping : AbstractAIMovingAndJumping
{
    private LinkedListNode<AbstractAIPathfinding.PathChainElement> _currentChain = null;

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterAIManager.AIPathfinding.OnPathUpdated += AIPathfinding_OnPathUpdated;
    }

    private void AIPathfinding_OnPathUpdated(object sender, System.EventArgs e)
    {
        _currentChain = CharComponents.CharacterAIManager.AIPathfinding.PathChain.First;
    }

    private void FixedUpdate()
    {
        UpdatePathTarget();
        UpdateActionsToReachPathTarget();
    }

    protected abstract void UpdatePathTarget();

    private void UpdateActionsToReachPathTarget()
    {
        Vector2Int characterTilePosition = TileManager.PositionToTilePosition(CharComponents.transform.position);

        if(_currentChain != null && _currentChain.Value.TargetPosition == characterTilePosition)
        {
            _currentChain = _currentChain.Next;
        }
        if (_currentChain == null)
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
            CharComponents.CharacterJumping.StopJump();
            return;
        }

        //moving to target
        if (characterTilePosition.x > _currentChain.Value.TargetPosition.x)
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Left);
        }
        else if (characterTilePosition.x < _currentChain.Value.TargetPosition.x)
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Right);
        }
        else
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
        }

        //jumping to target
        if (characterTilePosition.y < _currentChain.Value.TargetPosition.y && !CharComponents.CharacterJumping.GetIsJumping())
        {
            CharComponents.CharacterJumping.TryStartJump();
        }
    }

    private void OnDestroy()
    {
        CharComponents.CharacterAIManager.AIPathfinding.OnPathUpdated -= AIPathfinding_OnPathUpdated;
    }
}
