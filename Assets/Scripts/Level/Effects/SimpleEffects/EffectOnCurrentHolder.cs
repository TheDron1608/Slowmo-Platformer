using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class EffectOnCurrentHolder : AbstractEffectWithSender, IMultiplierableEffect
{
    public AbstractEffect EffectOnHolder;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedObject.TryGetComponent(out Holdable holdable) && holdable.CurrentHolder != null)
        {
            holdable.CurrentHolder.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectOnHolder, sender, EffectMultiplier);
        }
        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.TryGetComponent(out Holdable h) && h.CurrentHolder != null;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && EffectOnHolder.Equals((other as EffectOnCurrentHolder).EffectOnHolder);
    }
}