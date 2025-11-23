using UnityEngine;

public class DisarmAndStealMeleeProjectileOwner : AbstractMeleeProjectileDeflection
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (
            MeleeProjectile.Owner == null ||
            MeleeProjectile.Weapon == null ||
            MeleeProjectile.Weapon.GetComponent<Holdable>() == null ||
            MeleeProjectile.Owner.CurrentHoldObject != MeleeProjectile.Weapon.GetComponent<Holdable>()
            )
        {
            return;
        }

        if (sender.TryGetComponent(out AbstractProjectile projectile) && projectile.Owner != null)
        {
            if (MeleeProjectile.Owner.TryThrow(Vector2.zero))
            {
                MeleeProjectile.Weapon.GetComponent<Holdable>().Give(projectile.Owner);
            }
        }

        MeleeProjectile.RemoveProjectile();

        RemoveSelf();
    }
}
