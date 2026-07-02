using UnityEngine;

[CreateAssetMenu(fileName = "SellCursesAmountSingleTimeUnlockCondition", menuName = "CharacterUnlockConditions/SellCursesAmountSingleTimeUnlockCondition")]
public class SellCursesAmountSingleTimeUnlockCondition : AbstractCharacterUnlockCondition
{
    public int RequiedTotalSoldPrice = 300;

    public override bool UnlockCondition()
    {
        return SessionManager.Instance?.TempSession?.MaxSoldCurses >= RequiedTotalSoldPrice;
    }
}