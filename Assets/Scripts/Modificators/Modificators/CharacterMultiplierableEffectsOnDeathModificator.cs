using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableEffectsOnDeathModificator : AbstractMultiplierableModificator
{
    public TeamManager.Teams Team = TeamManager.Teams.PLAYER;
    public List<AbstractEffect> CharacterEffectsOnDeath;

    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        base.OnObjectSpawned(sender, e);

        if (e.TryGetComponent(out AbstractCharacterComponent character) && character.CharComponents.CharacterTeam.Team == Team)
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