using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAIManager : AbstractCharacterComponent
{
    [Header("AIBehaviour")]
    public AbstractAIAttacking Attacking;
    [Header("AIInfo")]
    public AbstractAINearestEnemyInfo NearestEnemyInfo = null;
}
