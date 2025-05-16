using UnityEngine;

public class GibLimb : AbstractCharacterLimbEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedLimbPart.CharPartHealth.TryGib(sender);

        RemoveSelf();
    }
}
