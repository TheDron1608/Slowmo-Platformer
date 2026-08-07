
using System.Collections.Generic;

public class GivePlayerHoldableIfUnarmed : AbstractModificator
{
    public List<Holdable> PossibleHoldables = new();

    public override void OnLevelGenerated()
    {
        foreach (var playerCharacter in TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers())
        {
            if (playerCharacter.CharComponents.CharacterHolding.CurrentHoldObject == null)
            {
                playerCharacter.CharComponents.CharacterHolding.GiveNewHoldable(NumberMath.PickRandomItem(PossibleHoldables));
            }
        }
    }
}