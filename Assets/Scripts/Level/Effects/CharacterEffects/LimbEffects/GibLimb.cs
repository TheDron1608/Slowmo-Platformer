using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class GibLimb : AbstractCharacterLimbEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedPart.GetComponent<CharacterLimbPart>()?.CharPartHealth.TryGib(sender);

        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            (affectWho.GetComponent<CharacterLimbPart>()?.CharPartHealth.Gibable ?? false);
    }
}
