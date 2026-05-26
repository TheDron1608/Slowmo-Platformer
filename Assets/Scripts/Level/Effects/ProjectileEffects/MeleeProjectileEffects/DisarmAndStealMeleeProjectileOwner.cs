using UnityEngine;

public class DisarmAndStealMeleeProjectileOwner : AbstractMeleeProjectileDeflection
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        Debug.Log("steal " + Projectile);
        if (
            MeleeProjectile.Owner == null ||
            MeleeProjectile.Weapon == null ||
            !MeleeProjectile.Weapon.TryGetComponent(out Holdable holdable) ||
            MeleeProjectile.Owner.CurrentHoldObject != holdable
            )
        {
            return;
        }

        if (sender.TryGetComponent(out AbstractProjectile projectile) && projectile.Owner != null)
        {
            projectile.Owner.CharComponents.CharacterHolding.ForceDisarm(projectile?.Owner);
        }

        MeleeProjectile.RemoveProjectile();

        RemoveSelf();
    }
}
