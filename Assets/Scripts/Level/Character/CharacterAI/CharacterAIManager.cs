using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAIManager : AbstractCharacterComponent
{
    [Header("AIBehaviour")]
    public AbstractAIAttacking Attacking;
    public AbstractAIReloading Reloading;
    public AbstractAIRolling Rolling;
    public AbstractAIMovingAndJumping MovingAndJumping;
    public AbstractAIPickingHoldables PickingHoldables;
    [Header("AIInfo")]
    public AbstractAINearestEnemyInfo NearestEnemyInfo = null;
    public AbstractAIPathfinding AIPathfinding = null;
    public AbstractAINearestPrefferedHoldable NearestPrefferedHoldable = null;
}
