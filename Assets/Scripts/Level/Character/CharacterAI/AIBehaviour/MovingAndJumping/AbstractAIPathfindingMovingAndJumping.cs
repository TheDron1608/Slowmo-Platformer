using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AbstractAIPathfindingMovingAndJumping : AbstractAIMovingAndJumping
{
    const float COLLISION_DETECTION_DISTANCE_PRECISSION = 0.05f;

    /// <summary>
    /// if true, character will try not to stand on single position with other characters
    /// </summary>
    public bool CanConflictPosition = true;

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

        //trying move left
        if (
            (_currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_ON_PLATFORM || characterTilePosition.x > _currentChain.Value.TargetPosition.x) &&
            CharComponents.transform.position.x > _currentChain.Value.TargetPosition.x + 1f - CharComponents.CharacterCollision.GetColliderSize().x / 2 - COLLISION_DETECTION_DISTANCE_PRECISSION +
                (characterTilePosition.y < _currentChain.Value.TargetPosition.y && _currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_UP ? 1f : 0f)
            )
        {
            //try stop moving if is conflicting target position with another character
            if (CanConflictPosition && CharComponents.CharacterCollision.CurrentCollidingCharacters.Find(
                (AbstractCharacterComponent collidingCharacter) => CharComponents.transform.position.x > collidingCharacter.transform.position.x)
                )
            {
                //stop move if moving at same direction with another character at same position
                if (CharComponents.CharacterCollision.CurrentCollidingCharacters.Find(
                    (AbstractCharacterComponent collidingCharacter) => collidingCharacter.CharComponents.CharacterMoving.GetCurrentMoveDirection() < 0f && CharComponents.CharacterMoving.Speed <= collidingCharacter.CharComponents.CharacterMoving.Speed)
                    )
                {
                    CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
                }
                //stop move and apply reaching chain target if path chain target is occured by another character
                else if (
                    CharComponents.CharacterCollision.CurrentCollidingCharacters.Find(GetIsBlockingTargetPosition)
                    )
                {
                    CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
                    OnReachedChainTarget();
                    return;
                }
                //move left successfully
                else
                {
                    CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Left);
                }
            }
            //move left successfully
            else
            {
                CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Left);
            }
        }
        //trying move right
        else if (
            (_currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_ON_PLATFORM || characterTilePosition.x < _currentChain.Value.TargetPosition.x) &&
            CharComponents.transform.position.x < _currentChain.Value.TargetPosition.x + CharComponents.CharacterCollision.GetColliderSize().x / 2 + COLLISION_DETECTION_DISTANCE_PRECISSION +
                (characterTilePosition.y < _currentChain.Value.TargetPosition.y && _currentChain.Value.Type == AbstractAIPathfinding.PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_UP ? -1f : 0f)
            )
        {
            //try stop moving if is conflicting target position with another character
            if (CanConflictPosition && CharComponents.CharacterCollision.CurrentCollidingCharacters.Find(
                (AbstractCharacterComponent collidingCharacter) => CharComponents.transform.position.x < collidingCharacter.transform.position.x)
                )
            {
                //stop move if moving at same direction with another character at same position
                if (CharComponents.CharacterCollision.CurrentCollidingCharacters.Find(
                    (AbstractCharacterComponent collidingCharacter) => collidingCharacter.CharComponents.CharacterMoving.GetCurrentMoveDirection() > 0f && CharComponents.CharacterMoving.Speed <= collidingCharacter.CharComponents.CharacterMoving.Speed)
                    )
                {
                    CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
                }
                //stop move and apply reaching chain target if path chain target is occured by another character
                else if (CharComponents.CharacterCollision.CurrentCollidingCharacters.Find(GetIsBlockingTargetPosition))
                {
                    CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.None);
                    OnReachedChainTarget();
                    return;
                }
                //move right successfully
                else
                {
                    CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Right);
                }
            }
            //move right successfully
            else
            {
                CharComponents.CharacterMoving.TryMove(CharacterMoving.MoveDirection.Right);
            }
        }
        //try finish moving
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

    private bool GetIsBlockingTargetPosition(AbstractCharacterComponent checkWho)
    {
        if (checkWho.CharComponents.CharacterMoving.GetCurrentMoveDirection() != 0)
        {
            return false;
        }
        else
        {
            Vector2Int selfTargetPosition = 
                _selfStateBehaviourAI?.Pathfinding.PathTarget != null ? 
                _selfStateBehaviourAI.Pathfinding.PathTarget.Value.Position : 
                TileManager.PositionToTilePosition(CharComponents.Center.transform.position);

            Vector2Int sameWithTargetPosition = 
                checkWho.CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.Pathfinding.PathTarget != null ? 
                checkWho.CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.Pathfinding.PathTarget.Value.Position : 
                TileManager.PositionToTilePosition(checkWho.CharComponents.Center.transform.position);

            return selfTargetPosition == sameWithTargetPosition;
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
