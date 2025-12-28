using System.Collections.Generic;

public class CharacterMultiplierableEffectsOnDeathModificator : AbstractMultiplierableModificator
{
    public TeamManager.Teams Team = TeamManager.Teams.PLAYER;
    public List<AbstractEffect> CharacterEffectsOnDeath;

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(Team).GetTeamMembers())
        {
            character.CharComponents.CharacterHealth.EffectsOnLethal.AddRange(CharacterEffectsOnDeath);
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(Team).GetTeamMembers())
        {
            foreach (AbstractEffect effect in CharacterEffectsOnDeath)
            {
                character.CharComponents.CharacterHealth.EffectsOnLethal.Remove(effect);
            }
        }
    }
}