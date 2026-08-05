using UnityEngine;

[CreateAssetMenu(fileName = "SurviveTimeUnlockCondition", menuName = "CharacterUnlockConditions/SurviveTimeUnlockCondition")]
public class SurviveTimeUnlockCondition : AbstractCharacterUnlockCondition
{
    public float SurviveTimeSeconds = 180f;

    public override bool UnlockCondition()
    {
        return
            DifficultyManager.Instance != null &&
            DifficultyManager.Instance.TotalDifficultyTime > SurviveTimeSeconds;
    }
}