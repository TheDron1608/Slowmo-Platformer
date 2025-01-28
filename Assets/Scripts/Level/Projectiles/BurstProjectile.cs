using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstProjectile : AbstractRangedProjectile
{
    /*
    /// <summary>
    /// if higher than 0, each projectile will spawn DurationBetweenBurstProjectiles seconds after previous spawned projectile
    /// </summary>
    public float DurationBetweenBurstProjectiles = 0.0667f;
    public int BurstProjectilesAmount = 3;
    /// <summary>
    /// this projectile is a spawned multilpe subProjectiles, they will be instanitiated to scene and added to SubProjectiles property
    /// </summary>
    public AbstractProjectile SubProjectileInstance;

    private List<AbstractProjectile> _subProjectiles;

    public List<AbstractProjectile> SubProjectiles
    {
        get => _subProjectiles;
        private set => _subProjectiles = value;
    }

    public override AbstractProjectile SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {
        base.SpawnProjectile(direction, accuracityMultiplier, weapon);

        BurstProjectile newBurstProjectile = new();

        for (int i = 0; i < BurstProjectilesAmount; i++)
        {
            StartCoroutine(CreateSubProjectileAfterDuration(DurationBetweenBurstProjectiles * i, direction, accuracityMultiplier));
        }

        return newBurstProjectile;
    }

    private IEnumerator CreateSubProjectileAfterDuration(float duration, Quaternion direction, float accuracityMultiplier)
    {
        yield return new WaitForSeconds(duration);

        if (
            Weapon != null &&
            Weapon.TryGetComponent(out RangedWeapon rangedWeapon) &&
            rangedWeapon.LoadedLivingAmmoLeft > 0
            )
        {
            rangedWeapon.SpendAmmo(1);
        }

        BurstProjectile newProjectile = Instantiate(this, Weapon.transform);
        newProjectile.MoveAlign = VectorMath.RandomizeQuarternion(direction, Accuracy * accuracityMultiplier);
    }
    */
}