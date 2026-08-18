
public interface IInvertableTeamModificator
{
    public bool InvertTeam { get; set; }

    public static TeamManager.Teams GetInvertedTeam(TeamManager.Teams baseTeam)
    {
        switch (baseTeam)
        {
            case TeamManager.Teams.PLAYER:
                return TeamManager.Teams.DEFAULT_ENEMY;
            case TeamManager.Teams.DEFAULT_ENEMY:
                return TeamManager.Teams.PLAYER;
            default:
                return baseTeam;
        }
    }
}