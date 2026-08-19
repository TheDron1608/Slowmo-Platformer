using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterMultiplierableEffectsOnStartModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> PlayerCharacterEffectsOnStart;
    public List<AbstractEffect> AltEffectsOnInverTeam;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        List<AbstractEffect> targetEffects = InvertTeam && AltEffectsOnInverTeam.Count > 0 ? AltEffectsOnInverTeam : PlayerCharacterEffectsOnStart;
        foreach (AbstractEffect effect in character.CharacterEffectsReceiver.ApplyEffect(targetEffects, null, ModificatorMultiplier, true))
        {
            if (effect is ITriggerableEffect triggerableEffect)
            {
                triggerableEffect.OnTriggered += TriggerableEffect_OnTriggered;
            }
        }
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        List<AbstractEffect> targetEffects = InvertTeam && AltEffectsOnInverTeam.Count > 0 ? AltEffectsOnInverTeam : PlayerCharacterEffectsOnStart;
        foreach (AbstractEffect effect in character.CharacterEffectsReceiver.CurrentEffects)
        {
            if (effect is ITriggerableEffect triggerableEffect && targetEffects.Contains(effect))
            {
                triggerableEffect.OnTriggered -= TriggerableEffect_OnTriggered;
            }
        }

        character.CharacterEffectsReceiver.RemoveEffect(targetEffects);
    }

    private void TriggerableEffect_OnTriggered(object sender, System.EventArgs e)
    {
        TryTriggerIconAnimation();
    }
}