using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class OnHoldedByTeamMemberHoldablesEffectsModificator : AbstractTeamHoldablesModificator
{
    public List<AbstractEffect> EffectsOnHoldedHoldables = new();

    protected override void OnAffectedHoldablePickedUp(Holdable holdable, CharacterHoldingObjects holder)
    {
        foreach (AbstractEffect effect in holdable.EffectsReceiver.ApplyEffect(EffectsOnHoldedHoldables, holder, ModificatorMultiplier))
        {
            if (effect is ITriggerableEffect triggerableEffect)
            {
                triggerableEffect.OnTriggered += TriggerableEffect_OnTriggered;
            }
        }
    }

    protected override void OnAffectedHoldableThrown(Holdable holdable, CharacterHoldingObjects thrower)
    {
        foreach (AbstractEffect effect in holdable.EffectsReceiver.CurrentEffects)
        {
            if (effect is ITriggerableEffect triggerableEffect && holdable.EffectsReceiver.CurrentEffects.Contains(effect))
            {
                triggerableEffect.OnTriggered -= TriggerableEffect_OnTriggered;
            }
        }

        holdable.EffectsReceiver.RemoveEffect(EffectsOnHoldedHoldables);
    }

    protected override void OnHoldableRemovedAffect(Holdable holdable)
    {
        foreach (AbstractEffect effect in holdable.EffectsReceiver.CurrentEffects)
        {
            if (effect is ITriggerableEffect triggerableEffect && holdable.EffectsReceiver.CurrentEffects.Contains(effect))
            {
                triggerableEffect.OnTriggered -= TriggerableEffect_OnTriggered;
            }
        }

        holdable.EffectsReceiver.RemoveEffect(EffectsOnHoldedHoldables);
    }

    protected override void OnHoldableAffected(Holdable holdable)
    {

    }

    private void TriggerableEffect_OnTriggered(object sender, System.EventArgs e)
    {
        TryTriggerIconAnimation();
    }
}