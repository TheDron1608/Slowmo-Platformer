using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableEffectsOnDeathReplaceDefaultModificator : CharacterMultiplierableEffectsOnDeathModificator
{
    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        if (e.TryGetComponent(out AbstractCharacterComponent character) && character.CharComponents.CharacterTeam.Team == Team)
        {
            for (int i = 0; i < character.CharComponents.CharacterHealth.DefaultEffectsOnLethal.Count; i++)
            {
                character.CharComponents.CharacterHealth.EffectsOnLethal.Remove(character.CharComponents.CharacterHealth.DefaultEffectsOnLethal[i]);
            }

            base.OnObjectSpawned(sender, e);
        }
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