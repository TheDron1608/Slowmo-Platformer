using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCompositeProjectile : AbstractProjectile
{
    public int SubProjectilesAmountOnSpawn = 6;
    /// <summary>
    /// this projectile is a spawned multilpe subProjectiles, they will be instanitiated to scene and added to SubProjectiles property
    /// </summary>
    public AbstractSingleProjectile SubProjectileInstance;

    public override float Damage
    {
        get => transform.GetComponentInChildren<AbstractSingleProjectile>().Damage;
        set
        {
            foreach (AbstractSingleProjectile singleProjectile in transform.GetComponentsInChildren<AbstractSingleProjectile>())
            {
                singleProjectile.Damage = value;
            }
        }
    }
    public override float AttackCooldown
    {
        get => transform.GetComponentInChildren<AbstractSingleProjectile>().AttackCooldown;
        set
        {
            foreach (AbstractSingleProjectile singleProjectile in transform.GetComponentsInChildren<AbstractSingleProjectile>())
            {
                singleProjectile.AttackCooldown = value;
            }
        }
    }
    public override float Accuracy
    {
        get => transform.GetComponentInChildren<AbstractSingleProjectile>().Accuracy;
        set
        {
            foreach (AbstractSingleProjectile singleProjectile in transform.GetComponentsInChildren<AbstractSingleProjectile>())
            {
                singleProjectile.Accuracy = value;
            }
        }
    }
    public override float KnockBack
    {
        get => transform.GetComponentInChildren<AbstractSingleProjectile>().KnockBack;
        set
        {
            foreach (AbstractSingleProjectile singleProjectile in transform.GetComponentsInChildren<AbstractSingleProjectile>())
            {
                singleProjectile.KnockBack = value;
            }
        }
    }
    public override ProjectilePiercing Pierce
    {
        get => transform.GetComponentInChildren<AbstractSingleProjectile>().Pierce;
        set
        {
            foreach (AbstractSingleProjectile singleProjectile in transform.GetComponentsInChildren<AbstractSingleProjectile>())
            {
                singleProjectile.Pierce = value;
            }
        }
    }
    public override CharacterHoldingObjects Owner
    {
        get => transform.GetComponentInChildren<AbstractSingleProjectile>().Owner;
    }
    public override Weapon Weapon
    {
        get => transform.GetComponentInChildren<AbstractSingleProjectile>().Weapon;
    }
}
