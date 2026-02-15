using UnityEngine;

public abstract class AbstractCharacterSpecial : AbstractCharacterComponent
{
    const TeamManager.Teams COST_AFFECT_ON_TEAM = TeamManager.Teams.PLAYER;

    public float HealthCost = 0f;
    public int ComboCost = 0;
    public bool IsAbleToDoSpecial = true;

    public bool GetHasEnoughForCost()
    {
        return
            CharComponents.CharacterTeam.Team != COST_AFFECT_ON_TEAM ||
            (
            CharComponents.CharacterHealth.CurrentHealth > HealthCost &&
            ScoreManager.Instance.CurrentCombo >= ComboCost
            );
    }

    public void SpendCost()
    {
        if (CharComponents.CharacterTeam.Team != COST_AFFECT_ON_TEAM) return;

        CharComponents.CharacterHealth.ApplyDamage(HealthCost, null);
        ScoreManager.Instance.CurrentCombo -= ComboCost;
    }
}