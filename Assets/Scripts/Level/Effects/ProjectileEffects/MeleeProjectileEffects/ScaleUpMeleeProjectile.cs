using UnityEngine;

public class ScaleUpMeleeProjectile : AbstractMeleeProjectileEffect
{
    public float ScaleMult = 1f;

    protected override void OnApply()
    {
        base.OnApply();
        Projectile.transform.localScale *= ScaleMult;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        Projectile.transform.localScale /= ScaleMult;
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            ScaleMult == (other as ScaleUpMeleeProjectile).ScaleMult;
    }
}
