using System.Collections.Generic;
using UnityEngine;

public class BuckshotProjectile : AbstractRangedProjectile
{
    public int BuckshotSubProjectilesAmount = 6;

    protected override List<AbstractProjectile> OnSpawnProjectile(Quaternion direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {
        if (weapon != null && weapon.TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SpendAmmo(1);
        }

        List<AbstractProjectile> result = new();
        for (int i = 0; i < BuckshotSubProjectilesAmount; i++)
        {
            BuckshotProjectile newProjectile = Instantiate(
                this, 
                weapon.transform.position, 
                direction, 
                LayerManager.Instance.GetZLayerOfGameObject(weapon.gameObject).ProjectilesContainer
                );

            newProjectile.MoveAlign = VectorMath.RandomizeQuarternion(
                direction,
                (Accuracy + (1 - Accuracy) * i / BuckshotSubProjectilesAmount) * accuracityMultiplier
            );

            newProjectile.Weapon = weapon;
            if (weapon != null && weapon.TryGetComponent(out Holdable holdableWeapon))
            {
                newProjectile.Owner = holdableWeapon.CurrentHolder;
            }
            else if (weapon != null && weapon.TryGetComponent(out UnarmedWeapon unarmedWeapon))
            {
                newProjectile.Owner = unarmedWeapon.CharComponents.CharacterHolding;
            }

            result.Add(newProjectile);
        }

        return result;
    }
}
