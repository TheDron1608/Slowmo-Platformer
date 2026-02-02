using UnityEngine;

public class OnDistanceToEnemyStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    [Header("Behaviour Condition Parameters")]
    public float MinDistance = 0f;
    public float MaxDistance = 20f;

    public override bool StateBehaviourCondition()
    {
        return
            CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.NearestEnemyInfo != null &&
            (
                (
                    (CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.NearestEnemyInfo.NearestEnemyDistance) >= MinDistance &&
                    (CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.NearestEnemyInfo.NearestEnemyDistance) <= MaxDistance
                ) ||
                (
                    CharComponents.CharacterAIManager.CurrentActiveStateBehaviour == this &&
                    (CharComponents.CharacterAIManager.CurrentActiveStateBehaviour?.Pathfinding.PathTarget?.Position) != null
                )
            );
    }
}
