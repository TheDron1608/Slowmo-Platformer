
using System;
using UnityEngine;

public interface IDamagable
{
    public float MaxHealth { get; }
    public float MinHealth { get; }
    public float CurrentHealth { get; }
    public void ApplyDamage(float damage, MonoBehaviour damager, float damageMultiplierMultiplier = 1f);
    public void SetHealth(float health, MonoBehaviour setter);
    public bool PiercableThrought { get; set; }
    public bool HitableByMeleeProjectiles { get; set; }
    public bool HitableByRangedProjectiles { get; set; }
    public float DamageMultiplier { get; set; }
    public bool UnlimitedHealth { get; set; }


    public event EventHandler<AbstractProjectile> OnHitByProjectile;

    public void ApplyMaxHealth(float newMaxHealth, MonoBehaviour applier);
    public void ApplyMinHealth(float newMinHealth, MonoBehaviour applier);
    public void ApplyProjectileHit(AbstractProjectile hitter, bool includeSound = true);

    public void RestoreHealth(MonoBehaviour restorer)
    {
        SetHealth(MaxHealth, restorer);
    }
}