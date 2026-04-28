using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class RestoreHealth : AbstractDamagableObjectEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedDamagableObject.RestoreHealth(sender);
    }
}
