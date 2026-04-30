using UnityEngine;

public abstract class AbstractMeleeProjectileDeflection : AbstractMeleeProjectileEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        MeleeProjectile.OnDeflected(sender);
    }
}
