using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FindUniqueHoldablesUnlockCondition", menuName = "CharacterUnlockConditions/FindUniqueHoldablesUnlockCondition")]
public class FindUniqueHoldablesUnlockCondition : AbstractCharacterUnlockCondition
{
    public List<Holdable> RequiredFindHoldables = new();

    public override bool UnlockCondition()
    {
        if (TeamManager.Instance == null || SessionManager.Instance?.CurrentSession == null) return false;

        bool foundAnyNewUniqueHoldable = false;
        foreach (CharacterTeam playerCharacter in TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers())
        {
            if (playerCharacter.CharComponents.CharacterHolding.CurrentHoldObject != null)
            {
                if (!SessionManager.Instance.CurrentSession.FoundUniqueHoldables.Contains(playerCharacter.CharComponents.CharacterHolding.CurrentHoldObject.FindingUniqueCodeName))
                {
                    SessionManager.Instance.CurrentSession.FoundUniqueHoldables.Add(playerCharacter.CharComponents.CharacterHolding.CurrentHoldObject.FindingUniqueCodeName);
                    foundAnyNewUniqueHoldable = true;
                }
            }
        }

        if (foundAnyNewUniqueHoldable)
        {
            return RequiredFindHoldables.All(e => SessionManager.Instance.CurrentSession.FoundUniqueHoldables.Contains(e.FindingUniqueCodeName));
        }
        else
        {
            return false;
        }
    }
}