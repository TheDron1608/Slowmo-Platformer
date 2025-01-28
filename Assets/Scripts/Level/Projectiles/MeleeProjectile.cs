using System.Collections.Generic;
using UnityEngine;

public class MeleeProjectile : AbstractProjectile
{
    public enum RangedProjectileDeflectionType
    {
        NO_DEFLECT,
        ABSORB_PROJECTILE,
        DEFLECT_PROJECTILE
    }
    public enum MeleerojectileDeflectionType
    {
        NO_DEFLECT,
        ABSORB_PROJECTILE,
        RESET_COOLDOWN,
        DISARM
    }

    public override List<AbstractProjectile> SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {
        MeleeProjectile newProjectile = Instantiate(this, weapon.transform);

        newProjectile.transform.rotation = VectorMath.RandomizeQuarternion(direction, Accuracy);

        newProjectile.Weapon = weapon;
        if (weapon != null && weapon.TryGetComponent(out Holdable holdableWeapon))
        {
            newProjectile.Owner = holdableWeapon.CurrentHolder;
        }

        return new List<AbstractProjectile>() { newProjectile };
    }

    private void Awake()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        layer.UpdateLayerForGameObject(gameObject);
    }
}
