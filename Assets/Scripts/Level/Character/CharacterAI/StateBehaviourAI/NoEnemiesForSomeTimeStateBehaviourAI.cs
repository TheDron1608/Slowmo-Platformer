using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoEnemiesForSomeTimeStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    [Header("Behaviour Condition Parameters")]
    public float AwaitTimeWithoutEnemiesToGetCalm = 5f;

    public override bool StateBehaviourCondition()
    {
        return 
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour != null &&
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo.NearestEnemy == null &&
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo.TimeSinceLastEnemyDetection > AwaitTimeWithoutEnemiesToGetCalm;
    }

}
