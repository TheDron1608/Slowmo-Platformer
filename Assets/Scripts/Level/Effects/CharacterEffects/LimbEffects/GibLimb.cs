using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class GibLimb : AbstractCharacterLimbEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedPart.TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.CharPartHealth.TryGib(sender);
        }

        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out CharacterLimbPart limbPart) &&
            limbPart.CharPartHealth.Gibable;
    }
}
