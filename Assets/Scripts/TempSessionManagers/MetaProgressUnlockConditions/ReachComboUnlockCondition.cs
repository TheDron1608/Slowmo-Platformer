using UnityEngine;

[CreateAssetMenu(fileName = "ReachComboUnlockCondition", menuName = "CharacterUnlockConditions/ReachComboUnlockCondition")]
public class ReachComboUnlockCondition : AbstractCharacterUnlockCondition
{
    public int RequiedCombo = 50;

    public override bool UnlockCondition()
    {
        return
            ScoreManager.Instance != null &&
            ScoreManager.Instance.CurrentCombo >= RequiedCombo;
    }
}