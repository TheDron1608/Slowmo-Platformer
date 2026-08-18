
using System.Collections.Generic;

public class GivePlayerHoldableIfUnarmed : AbstractModificator, IInvertableTeamModificator
{
    public TeamManager.Teams AffectTeam = TeamManager.Teams.PLAYER;
    public List<Holdable> PossibleHoldables = new();

    private bool _invertTeam = false;
    public bool InvertTeam
    {
        get => _invertTeam;
        set
        {
            if (_invertTeam == value) return;
            _invertTeam = value;

            if (!DisabledModificator)
            {
                OnModificatorRemoved();
                OnModificatorAdded();
            }
        }
    }

    public override void OnLevelGenerated()
    {
        foreach (var playerCharacter in TeamManager.Instance.GetTeamDataByTeam(InvertTeam ? IInvertableTeamModificator.GetInvertedTeam(AffectTeam) : AffectTeam).GetTeamMembers())
        {
            if (playerCharacter.CharComponents.CharacterHolding.CurrentHoldObject == null)
            {
                playerCharacter.CharComponents.CharacterHolding.GiveNewHoldable(NumberMath.PickRandomItem(PossibleHoldables));
            }
        }
    }
}