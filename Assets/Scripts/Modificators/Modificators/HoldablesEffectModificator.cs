using System.Collections.Generic;
using UnityEngine.TextCore.Text;

public class HoldablesEffectModificator : AbstractHoldablesModificator
{
    public List<AbstractEffect> Effects = new();

    protected override void OnHoldableAffected(Holdable holdable)
    {
        foreach (AbstractEffect effect in holdable.EffectsReceiver.ApplyEffect(Effects, null, ModificatorMultiplier))
        {
            if (effect is ITriggerableEffect triggerableEffect)
            {
                triggerableEffect.OnTriggered += TriggerableEffect_OnTriggered;
            }
        }
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

        holdable.EffectsReceiver.RemoveEffect(Effects);
    }

    private void TriggerableEffect_OnTriggered(object sender, System.EventArgs e)
    {
        TryTriggerIconAnimation();
    }

    protected override void OnAffectedHoldablePickedUp(Holdable holdable, CharacterHoldingObjects holder)
    {

    }

    protected override void OnAffectedHoldableThrown(Holdable holdable, CharacterHoldingObjects thrower)
    {

    }
}