using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class RandomListEffect : AbstractEffectWithSender, IDelayedEffect, IMultiplierableEffect
{
    public List<AbstractEffect> EffectsList = new();

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedObject.ApplyEffect(
            NumberMath.PickRandomItem(EffectsList),
            sender,
            EffectMultiplier
            );

        RemoveSelf();
    }

    public override List<AbstractEffect> GetSelfIncludeIncomingEffects()
    {
        List<AbstractEffect> result = base.GetSelfIncludeIncomingEffects();
        EffectsList.ForEach(effect => result.AddRange(effect.GetSelfIncludeIncomingEffects()));

        return result;
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            EffectsList.TrueForAll(effect => EffectsList.Contains(effect));
    }
}
