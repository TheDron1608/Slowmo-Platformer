using UnityEngine;

public class NoEnemiesForSomeTimeStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    [Header("Behaviour Condition Parameters")]
    public float AwaitTimeWithoutEnemiesToGetCalm = 5f;

    private bool _noEnemies = true;

    public override bool StateBehaviourCondition()
    {
        if (
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.NearestEnemyInfo.NearestEnemy != null ||
            (NearestEnemyInfo.LastHeardEnemy != null && NearestEnemyInfo.TimeSinceLastHeardEnemy < AwaitTimeWithoutEnemiesToGetCalm)
            )
        {
            _noEnemies = false;
        }
        else if (CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.NearestEnemyInfo.TimeSinceLastEnemyDetection > AwaitTimeWithoutEnemiesToGetCalm)
        {
            _noEnemies = true;
        }

        return _noEnemies;
    }

}
