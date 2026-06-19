using UnityEngine;

public class NoEnemiesForSomeTimeStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    [Header("Behaviour Condition Parameters")]
    public float AwaitTimeWithoutEnemiesToGetCalm = 5f;
    public bool IgnoreHearingNoise = false;

    private bool _noEnemies = true;

    public override bool StateBehaviourCondition()
    {
        if (
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.NearestEnemyInfo.NearestEnemy != null ||
            (!IgnoreHearingNoise && NearestEnemyInfo.LastHeardEnemy != null)
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
