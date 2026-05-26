using UnityEngine;

public class Break : AbstractEffectWithSender, ILethalEffect
{
    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedObject.TryGetComponent(out BreakableObject breakable))
        {
            breakable.BreakObject(sender);
        }

        RemoveSelf();
    }
}
