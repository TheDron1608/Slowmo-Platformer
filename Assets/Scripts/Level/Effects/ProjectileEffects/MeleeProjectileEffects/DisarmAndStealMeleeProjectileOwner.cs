using UnityEngine;

public class DisarmAndStealMeleeProjectileOwner : AbstractMeleeProjectileDeflection
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        Debug.Log("steal " + Projectile);
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
            projectile.Owner.CharComponents.CharacterHolding.ForceDisarm(projectile?.Owner);
        }

        MeleeProjectile.RemoveProjectile();

        RemoveSelf();
    }
}
