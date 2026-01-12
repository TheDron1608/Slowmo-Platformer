using System.Collections.Generic;

public class CharacterMultiplierableEffectsOnStartModificator : AbstractMultiplierableModificator
{
    public TeamManager.Teams Team = TeamManager.Teams.PLAYER;
    public List<AbstractEffect> PlayerCharacterEffectsOnStart;
    

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(Team).GetTeamMembers())
        {
            foreach(AbstractEffect effect in character.CharComponents.CharacterEffectsReceiver.ApplyEffect(PlayerCharacterEffectsOnStart, null, ModificatorMultiplier))
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
            character.CharComponents.CharacterEffectsReceiver.RemoveEffect(PlayerCharacterEffectsOnStart);
        }
    }

    private void TriggerableEffect_OnTriggered(object sender, System.EventArgs e)
    {
        TryTriggerIconAnimation();
    }
}