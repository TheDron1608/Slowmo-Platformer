
using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class EffectOnHardStunOnly : AbstractEffectWithSender, IMultiplierableEffect
{
    public List<AbstractEffect> EffectsOnHasStun = new();
    public List<AbstractEffect> EffectsOnNoStun = new();

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedObject.ApplyEffect(
            AffectedObject.GetHasEffect<HardStun>() ? EffectsOnHasStun : EffectsOnNoStun,
            sender,
            EffectMultiplier
            );

        RemoveSelf();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            EffectsOnHasStun == (other as EffectOnHardStunOnly).EffectsOnHasStun &&
            EffectsOnNoStun == (other as EffectOnHardStunOnly).EffectsOnNoStun;
    }
}