using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : AbstractRangedProjectile
{
    public override AbstractProjectile SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {
        base.SpawnProjectile(direction, accuracityMultiplier, weapon);

        if (weapon != null && weapon.TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SpendAmmo(1);
        }

        BulletProjectile newProjectile = Instantiate(this, weapon.transform);
        newProjectile.MoveAlign = VectorMath.RandomizeQuarternion(direction, Accuracy * accuracityMultiplier);

        return newProjectile;
    }
}
