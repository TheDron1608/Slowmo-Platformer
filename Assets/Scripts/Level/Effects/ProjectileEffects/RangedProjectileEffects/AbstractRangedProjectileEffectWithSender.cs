using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractRangedProjectileEffectWithSender : AbstractProjectileEffectWithSender, IRangedProjectileEffect
{
    private AbstractRangedProjectile _rangedProjectile;

    public AbstractRangedProjectile RangedProjectile
    {
        get => _rangedProjectile;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<AbstractRangedProjectile>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _rangedProjectile)) throw new UnityException("AbstractRangedProjectile component not found at " + gameObject.name);
    }
}
