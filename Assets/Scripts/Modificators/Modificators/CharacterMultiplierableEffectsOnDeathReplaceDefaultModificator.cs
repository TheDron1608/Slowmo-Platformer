using System.Collections.Generic;

public class CharacterMultiplierableEffectsOnDeathReplaceDefaultModificator : CharacterMultiplierableEffectsOnDeathModificator
{
    public override void OnLevelGenerated()
    {
        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(Team).GetTeamMembers())
        {
            for (int i = 0; i < character.CharComponents.CharacterHealth.DefaultEffectsOnLethal.Count; i++)
            {
                character.CharComponents.CharacterHealth.EffectsOnLethal.Remove(character.CharComponents.CharacterHealth.DefaultEffectsOnLethal[i]);
            }
        }

        base.OnLevelGenerated();
    }

    public override void OnModificatorRemoved()
    {
        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(Team).GetTeamMembers())
        {
            foreach (AbstractEffect defaultEffect in character.CharComponents.CharacterHealth.DefaultEffectsOnLethal)
            {
                if (!character.CharComponents.CharacterHealth.EffectsOnLethal.Contains(defaultEffect))
                {
                    character.CharComponents.CharacterHealth.EffectsOnLethal.Add(defaultEffect);
                }
            }
        }

        base.OnModificatorRemoved();
    }
}