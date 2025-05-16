using UnityEngine;

public class GibLimb : AbstractCharacterLimbEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedPart.GetComponent<CharacterLimbPart>()?.CharPartHealth.TryGib(sender);

        RemoveSelf();
    }
}
