using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class Break : AbstractEffectWithSender, ILethalEffect
{
    public bool BreakEntirely = false;

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedObject.TryGetComponent(out BreakableObject breakable))
        {
            if (BreakEntirely && breakable is IBreakableEntirelyObject entireblyBreakable)
            {
                entireblyBreakable.BreakObjectEntirely(sender);
            }
            else
            {
                breakable.BreakObject(sender);
            }
        }

        RemoveSelf();
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && BreakEntirely == (other as Break).BreakEntirely;
    }
}
