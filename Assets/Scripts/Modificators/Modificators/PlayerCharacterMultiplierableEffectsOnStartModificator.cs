using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableEffectsOnStartModificator : AbstractMultiplierableModificator
{
    public TeamManager.Teams Team = TeamManager.Teams.PLAYER;
    public List<AbstractEffect> PlayerCharacterEffectsOnStart;
    

    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        base.OnObjectSpawned(sender, e);

        if (e.TryGetComponent(out AbstractCharacterComponent character) && character.CharComponents.CharacterTeam.Team == Team)
        {
            foreach (AbstractEffect effect in character.CharComponents.CharacterEffectsReceiver.ApplyEffect(PlayerCharacterEffectsOnStart, null, ModificatorMultiplier))
            {
                if (effect is ITriggerableEffect triggerableEffect)
                {
                    triggerableEffect.OnTriggered += TriggerableEffect_OnTriggered;
                }
            }
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(Team).GetTeamMembers())
        {
            foreach(AbstractEffect effect in character.CharComponents.CharacterEffectsReceiver.CurrentEffects)
            {
                if (PlayerCharacterEffectsOnStart.Contains(effect) && effect is ITriggerableEffect triggerableEffect)
                {
                    triggerableEffect.OnTriggered -= TriggerableEffect_OnTriggered;
                }
            }

            character.CharComponents.CharacterEffectsReceiver.RemoveEffect(PlayerCharacterEffectsOnStart);
        }
    }

    private void TriggerableEffect_OnTriggered(object sender, System.EventArgs e)
    {
        TryTriggerIconAnimation();
    }
}