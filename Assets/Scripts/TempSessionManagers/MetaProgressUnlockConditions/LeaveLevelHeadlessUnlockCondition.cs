
using UnityEngine;

[CreateAssetMenu(fileName = "LeaveLevelHeadlessUnlockCondition", menuName = "CharacterUnlockConditions/LeaveLevelHeadlessUnlockCondition")]
public class LeaveLevelHeadlessUnlockCondition : AbstractCharacterUnlockCondition
{
    public override bool UnlockCondition()
    {
        if (TeamManager.Instance == null) return false;

        foreach (CharacterTeam playerCharacter in TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers())
        {
            if (
                playerCharacter.CharComponents.CharacterInteract.LastInteractObject is AbstractLevelFinishDoor &&
                playerCharacter.CharComponents.CharacterPartsManager.GetCharacterPart(CharacterPart.PartTypes.HEAD) == null
                )
            {
                return true;
            }
        }

        return false;
    }
}