using System.Collections.Generic;
using UnityEngine;

public class CharacterEffectOnSpawnModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> Effects;
    public List<AbstractEffect> AltEffectsOnInvertTeam;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        List<AbstractEffect> addedEffects = character.CharacterEffectsReceiver.ApplyEffect(
            InvertTeam && AltEffectsOnInvertTeam.Count > 0 ? AltEffectsOnInvertTeam : Effects, 
            null, 
            ModificatorMultiplier, 
            true
            );

        foreach (AbstractEffect effect in addedEffects)
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
            if (effect is ITriggerableEffect triggerableEffect)
            {
                triggerableEffect.OnTriggered -= TriggerableEffect_OnTriggered;
            }
        }
        foreach (AbstractEffect effect in InvertTeam && AltEffectsOnInvertTeam.Count > 0 ? AltEffectsOnInvertTeam : Effects)
        {
            character.CharacterEffectsReceiver.RemoveEffect(effect);
        }
    }

    private void TriggerableEffect_OnTriggered(object sender, System.EventArgs e)
    {
        CurrentIcon?.TriggerAnimation();
    }
}