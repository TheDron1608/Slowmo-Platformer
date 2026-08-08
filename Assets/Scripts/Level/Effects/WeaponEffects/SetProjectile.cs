using System.Collections.Generic;
using UnityEngine;

public class SetProjectile : AbstractWeaponEffect
{
    public List<AbstractProjectile> Projectiles;

    private AbstractProjectile _oldProjectile = null;

    protected override void OnApply()
    {
        base.OnApply();

        AffectedObject.RemoveEffect<SetProjectile>();

        _oldProjectile = Weapon.Projectile;
        Weapon.Projectile = NumberMath.PickRandomItem(Projectiles);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        Weapon.Projectile = _oldProjectile;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && Projectiles == (other as SetProjectile).Projectiles;
    }
}
