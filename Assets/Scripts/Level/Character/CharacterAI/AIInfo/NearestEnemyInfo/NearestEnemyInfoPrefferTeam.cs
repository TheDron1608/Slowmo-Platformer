using UnityEngine;

public class NearestEnemyInfoPrefferTeam : DefaultNearestEnemyInfo
{
    public TeamManager.Teams PrefferedTeam = TeamManager.Teams.PLAYER;

    protected override bool CharacterCondition(CharacterComponentsManager character)
    {
        return base.CharacterCondition(character) && character.CharacterTeam.Team == PrefferedTeam;
    }
}
