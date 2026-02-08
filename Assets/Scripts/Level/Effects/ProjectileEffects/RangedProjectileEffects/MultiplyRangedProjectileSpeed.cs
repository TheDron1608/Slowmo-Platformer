
public class MultiplyRangedProjectileSpeed : AbstractRangedProjectileEffect
{
    public float ProjectileSpeedMult = 1f;

    protected override void OnApply()
    {
        base.OnApply();

        RangedProjectile.BulletSpeed *= ProjectileSpeedMult;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        RangedProjectile.BulletSpeed /= ProjectileSpeedMult;
    }
}
