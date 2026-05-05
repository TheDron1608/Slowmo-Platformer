using UnityEngine;

[CreateAssetMenu(fileName = "SingleRunKillAmountUnlockCondition", menuName = "CharacterUnlockConditions/SingleRunKillAmountUnlockCondition")]
public class SingleRunKillAmountUnlockCondition : AbstractCharacterUnlockCondition
{
    public int RequiedKills = 200;

    public override bool UnlockCondition()
    {
        return SessionManager.Instance?.TempSession?.CurrentKills > RequiedKills;
    }
}