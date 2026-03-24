using System.Collections.Generic;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class RandomChancedEffect : AbstractEffectWithSender, IDelayedEffect, IMultiplierableEffect
{
    public AbstractEffect RandomEffect;
    public float RandomEffectChance = 0.5f;
    public RandomManager.ProcChanceTypes RandomType;
    public AbstractEffect ElseEffect;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedObject.ApplyEffect(
            RandomManager.Instance.ProcRandomChance(RandomEffectChance, RandomType) ? RandomEffect : ElseEffect,
            sender,
            EffectMultiplier
            );
    }

    public override List<AbstractEffect> GetSelfIncludeIncomingEffects()
    {
        return NumberMath.MergeLists(
            base.GetSelfIncludeIncomingEffects(), 
            RandomEffect?.GetSelfIncludeIncomingEffects(), 
            ElseEffect?.GetSelfIncludeIncomingEffects()
            );
    }

    public override bool Equals(AbstractEffect other)
    {
        return 
            base.Equals(other) && 
            RandomEffectChance == (other as RandomChancedEffect).RandomEffectChance &&
            RandomType == (other as RandomChancedEffect).RandomType &&
            (RandomEffect?.Equals((other as RandomChancedEffect).RandomEffect) ?? (other as RandomChancedEffect).RandomEffect == RandomEffect) &&
            (ElseEffect?.Equals((other as RandomChancedEffect).ElseEffect) ?? (other as RandomChancedEffect).ElseEffect == ElseEffect);
    }
}
