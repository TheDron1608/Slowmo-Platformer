using System;
using UnityEngine;

public class CounterDamageOnDamager : AbstractCharacterEffect, IEntireCharacterEffect, ITriggerableEffect
{
    public event EventHandler OnTriggered;

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterEffectsReceiver.OnEffectAddedIgnoreImmunity += AffectedCharacter_OnEffectAddedIgnoreImmunity;
        foreach (var charPart in AffectedCharacter.CharacterPartsManager.CharacterParts)
        {
            if (charPart is CharacterLimbPart limbPart)
            {
                limbPart.CharPartEffectsReceiver.OnEffectAddedIgnoreImmunity += AffectedCharacterLimb_OnEffectAddedIgnoreImmunity;
            }
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterEffectsReceiver.OnEffectAddedIgnoreImmunity -= AffectedCharacter_OnEffectAddedIgnoreImmunity;
        foreach (var charPart in AffectedCharacter.CharacterPartsManager.CharacterParts)
        {
            if (charPart is CharacterLimbPart limbPart)
            {
                limbPart.CharPartEffectsReceiver.OnEffectAddedIgnoreImmunity -= AffectedCharacterLimb_OnEffectAddedIgnoreImmunity;
            }
        }
    }

    private void AffectedCharacter_OnEffectAddedIgnoreImmunity(object sender, ObjectEffectsReceiver.EffectAddedEventArgs e)
    {
        if (e.Effect is Damage || e.Effect is AbstractStun || e.Effect is Gib || e.Effect is MitosisGib)
        {
            AbstractCharacterComponent characterSender = ObjectEffectsReceiver.TryGetCharacterFromSender(e.Sender);
            if (characterSender != null)
            {
                characterSender.CharComponents.CharacterEffectsReceiver.ApplyEffect(e.Effect, AffectedCharacter, 1f, true);
                OnTriggered?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void AffectedCharacterLimb_OnEffectAddedIgnoreImmunity(object sender, ObjectEffectsReceiver.EffectAddedEventArgs e)
    {
        if (e.Effect is Damage)
        {
            AbstractCharacterComponent characterSender = ObjectEffectsReceiver.TryGetCharacterFromSender(e.Sender);
            if (characterSender != null)
            {
                characterSender.CharComponents.CharacterEffectsReceiver.ApplyEffect(e.Effect, AffectedCharacter, 1f, true);
                OnTriggered?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (
            (e.Effect is GibLimb || e.Effect is CutOffLimb) &&
            ((sender as MonoBehaviour)?.TryGetComponent(out CharacterLimbPart limbPart) ?? false)
            )
        {
            AbstractCharacterComponent characterSender = ObjectEffectsReceiver.TryGetCharacterFromSender(e.Sender);
            CharacterLimbPart limbPartSender = characterSender?.CharComponents.CharacterPartsManager.GetCharacterPart(limbPart.PartType) as CharacterLimbPart;
            if (characterSender != null && limbPartSender != null)
            {
                limbPartSender.CharPartEffectsReceiver.ApplyEffect(e.Effect, AffectedCharacter, 1f, true);
                OnTriggered?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
