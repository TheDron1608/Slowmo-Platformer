using UnityEngine;

public class CutOffLimb : AbstractCharacterLimbEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedLimbPart.TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.CharPartHealth.TryCutOff(sender);
        }

        RemoveSelf();
    }
}
