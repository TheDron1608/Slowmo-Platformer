using UnityEngine;

public abstract class AbstractMeleeProjectileEffect : AbstractProjectileEffect, IMeleeProjectileEffect
{
    private MeleeProjectile _meleeProjectile;

    public MeleeProjectile MeleeProjectile
    {
        get => _meleeProjectile;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out MeleeProjectile mp);
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _meleeProjectile)) throw new UnityException("MeleeProjectile component not found at " + gameObject.name);
    }
}
