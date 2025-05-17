using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectileEffect : AbstractEffect, IProjectileEffect
{
    private AbstractProjectile _projectile;

    public AbstractProjectile Projectile
    {
        get => _projectile;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<AbstractProjectile>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _projectile)) throw new UnityException("AbstractProjectile component not found at " + gameObject.name);
    }
}
