using UnityEngine;

public abstract class AbstractRangedProjectileEffectWithSender : AbstractProjectileEffectWithSender, IRangedProjectileEffect
{
    private RangedProjectile _rangedProjectile;

    public RangedProjectile RangedProjectile
    {
        get => _rangedProjectile;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out RangedProjectile rp);
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _rangedProjectile)) throw new UnityException("AbstractRangedProjectile component not found at " + gameObject.name);
    }
}
