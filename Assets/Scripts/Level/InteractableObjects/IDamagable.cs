
using System;
using UnityEngine;

interface IDamagable
{
    public void ApplyDamage(float damage, MonoBehaviour damager);
    public bool PiercableThrought { get; set; }
    public bool HitableByMeleeProjectiles { get; set; }
    public bool HitableByRangedProjectiles { get; set; }

    public event EventHandler<AbstractProjectile> OnHitByProjectile;

    public void ApplyProjectileHit(AbstractProjectile hitter);
}