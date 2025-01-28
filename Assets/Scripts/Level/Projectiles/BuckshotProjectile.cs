using System.Collections.Generic;
using UnityEngine;

public class BuckshotProjectile : AbstractCompositeProjectile
{
    public float BuckshotAccuracyMultiplier = 0.85f;

    public override AbstractProjectile SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {
        if (weapon != null && weapon.TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SpendAmmo(1);
        }

        BuckshotProjectile newBuckshotProjectile = Instantiate(this, weapon.transform);
        for (int i = 0; i < SubProjectilesAmountOnSpawn; i++)
        {
            AbstractSingleProjectile newProjectile = Instantiate(SubProjectileInstance, newBuckshotProjectile.transform);

            if (newProjectile.TryGetComponent(out AbstractRangedProjectile rangedProjectile))
            {
                rangedProjectile.MoveAlign = VectorMath.RandomizeQuarternion(
                    direction,
                    (SubProjectileInstance.Accuracy * BuckshotAccuracyMultiplier + (1 - SubProjectileInstance.Accuracy * BuckshotAccuracyMultiplier) * i / SubProjectilesAmountOnSpawn) * accuracityMultiplier
                    );
            }

            Debug.Log(newBuckshotProjectile);
        }

        return newBuckshotProjectile;
    }
}
