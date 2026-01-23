using UnityEngine;

public abstract class AbstractRangedProjectileDeflection : AbstractRangedProjectileEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        Projectile.OnDeflected(sender);
    }
}
