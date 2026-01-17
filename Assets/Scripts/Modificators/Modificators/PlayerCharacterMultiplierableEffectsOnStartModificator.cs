using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterMultiplierableEffectsOnStartModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> PlayerCharacterEffectsOnStart;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        foreach (AbstractEffect effect in character.CharacterEffectsReceiver.ApplyEffect(PlayerCharacterEffectsOnStart, null, ModificatorMultiplier))
        {
            if (effect is ITriggerableEffect triggerableEffect)
            {
                triggerableEffect.OnTriggered += TriggerableEffect_OnTriggered;
            }
        }
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        foreach (AbstractEffect effect in character.CharacterEffectsReceiver.CurrentEffects)
        {
            if (PlayerCharacterEffectsOnStart.Contains(effect) && effect is ITriggerableEffect triggerableEffect)
            {
                triggerableEffect.OnTriggered -= TriggerableEffect_OnTriggered;
            }
        }

        character.CharacterEffectsReceiver.RemoveEffect(PlayerCharacterEffectsOnStart);
    }

    private void TriggerableEffect_OnTriggered(object sender, System.EventArgs e)
    {
        TryTriggerIconAnimation();
    }
}