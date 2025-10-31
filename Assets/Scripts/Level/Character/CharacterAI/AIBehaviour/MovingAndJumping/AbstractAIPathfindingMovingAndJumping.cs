using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public abstract class AbstractAIPathfindingMovingAndJumping : AbstractAIMovingAndJumping
{
    const float COLLISION_DETECTION_DISTANCE_PRECISSION = 0.05f;

    private LinkedListNode<AbstractAIPathfinding.PathChainElement> _currentChain = null;

    private void FixedUpdate()
    {
        UpdatePathTarget();
        UpdateActionsToReachPathTarget();
    }

    protected abstract void UpdatePathTarget();

    private void UpdateActionsToReachPathTarget()
    {
        Vector2Int characterTilePosition = TileManager.PositionToTilePosition(CharComponents.transform.position);

        if (_currentChain?.List != _selfStateBehaviourAI.Pathfinding.PathChain && CharComponents.CharacterCollision.IsCollidingFloor())
        {
            _currentChain = _selfStateBehaviourAI.Pathfinding.PathChain?.First;
        }
        else if (_selfStateBehaviourAI.Pathfinding.PathTarget.HasValue && _selfStateBehaviourAI.Pathfinding.PathTarget.Value.Position == characterTilePosition)
        {
            _currentChain = null;
        }

        //if no target stop moving and jumping
        if (_currentChain == null)
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
            CharComponents.CharacterJumping.StopJump();
            return;
        }

        //moving to target
        if (
            (_currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_ON_PLATFORM || characterTilePosition.x > _currentChain.Value.TargetPosition.x) &&
            CharComponents.transform.position.x > _currentChain.Value.TargetPosition.x + 1f - CharComponents.CharacterCollision.GetColliderSize().x / 2 - COLLISION_DETECTION_DISTANCE_PRECISSION +
                (characterTilePosition.y < _currentChain.Value.TargetPosition.y && _currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_UP ? 1f : 0f)
            )
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Left);
        }
        else if (
            (_currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_ON_PLATFORM || characterTilePosition.x < _currentChain.Value.TargetPosition.x) &&
            CharComponents.transform.position.x < _currentChain.Value.TargetPosition.x + CharComponents.CharacterCollision.GetColliderSize().x / 2 + COLLISION_DETECTION_DISTANCE_PRECISSION +
                (characterTilePosition.y < _currentChain.Value.TargetPosition.y && _currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_UP ? -1f : 0f)
            )
        {
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Right);
        }
        else 
        {
            if (characterTilePosition.y == _currentChain.Value.TargetPosition.y && CharComponents.CharacterCollision.IsCollidingFloor())
            {
                OnReachedChainTarget();
                return;
            }
            CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
        }

        //jumping to target
        if (
            CharComponents.CharacterCollision.IsCollidingFloor() && 
            !CharComponents.CharacterJumping.GetIsJumping() &&
            (
                _currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_UP ||
                _currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_MIDDLE
            )
            )
        {
            if (CharComponents.CharacterMoving.GetIsNeedChangeClumsyDirection(_currentChain.Value.TargetPosition.x - characterTilePosition.x))
            {
                CharComponents.CharacterMoving.TrySetClumsyAlign(_currentChain.Value.TargetPosition.x - characterTilePosition.x, true);
            }
            else
            {
                CharComponents.CharacterJumping.TryStartJump();
            }
        }
        else if (
            _currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_MIDDLE  &&
            math.abs(characterTilePosition.x - _currentChain.Value.TargetPosition.x) <= CharComponents.CharacterJumping.GetJumpWidth() / 2
            )
        {
            CharComponents.CharacterJumping.StopJump();
        }
    }

    private void OnReachedChainTarget()
    {
        if (_currentChain.Value.RequiredIteractableToContinue)
        {
            if (!CharComponents.CharacterInteract.TryInteract(_currentChain.Value.RequiredIteractableToContinue))
            {
                _selfStateBehaviourAI.Pathfinding.PathTarget = null;
            }
        }

        _currentChain = _currentChain?.Next;
    }
}
