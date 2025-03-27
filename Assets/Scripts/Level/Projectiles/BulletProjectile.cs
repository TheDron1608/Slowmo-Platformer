using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : AbstractRangedProjectile
{
    protected override List<AbstractProjectile> OnSpawnProjectile(Quaternion direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {
        if (weapon != null && weapon.TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SpendAmmo(1);
        }

        BulletProjectile newProjectile = Instantiate(
                this,
                weapon.transform.position,
                direction,
                LayerManager.Instance.GetZLayerOfGameObject(weapon.gameObject).transform
                );

        newProjectile.transform.position = weapon.transform.position;

        newProjectile.MoveAlign = VectorMath.RandomizeQuarternion(direction, Accuracy * accuracityMultiplier);

        newProjectile.Weapon = weapon;
        if (weapon != null && weapon.TryGetComponent(out Holdable holdableWeapon))
        {
            newProjectile.Owner = holdableWeapon.CurrentHolder;
        }
        else if (weapon != null && weapon.TryGetComponent(out UnarmedWeapon unarmedWeapon))
        {
            newProjectile.Owner = unarmedWeapon.CharComponents.CharacterHolding;
        }


        return new List<AbstractProjectile>() { newProjectile };
    }
}
