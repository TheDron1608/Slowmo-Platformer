using UnityEngine;

public class SwapProjectile : AbstractWeaponEffect
{
    public AbstractProjectile ReplacedProjectile;
    public AbstractProjectile ReplaceProjectile;

    private AbstractProjectile _oldProjectile = null;

    protected override void OnApply()
    {
        base.OnApply();

        _oldProjectile = Weapon.Projectile;
        Weapon.Projectile = ReplaceProjectile;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        Weapon.Projectile = _oldProjectile;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out Weapon weapon) &&
            weapon.Projectile == ReplacedProjectile;
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            ReplacedProjectile == (other as SwapProjectile).ReplacedProjectile &&
            ReplaceProjectile == (other as SwapProjectile).ReplaceProjectile;
    }
}
