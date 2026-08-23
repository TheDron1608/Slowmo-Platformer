
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class ForceOpenDoor : AbstractEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedObject.TryGetComponent(out OnInteractToggleOpenDoor door))
        {
            door.ForceOpen(sender?.gameObject);
        }
        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.TryGetComponent(out OnInteractToggleOpenDoor d);
    }
}