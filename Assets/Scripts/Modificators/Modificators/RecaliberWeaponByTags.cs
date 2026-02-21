using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecaliberWeaponByTags : AbstractModificator
{
    public Weapon.WEAPON_TAGS[] Tag;
    public List<AbstractProjectile> ReplaceProjectiles = new();


    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        base.OnObjectSpawned(sender, e);

        if (
            e.TryGetComponent(out Weapon weapon) &&
            e.TryGetComponent(out Holdable holdableWeapon) &&
            Tag.All(tag => weapon.Tags.Contains(tag))
            ) 
        {
            weapon.Projectile = NumberMath.PickRandomItem(ReplaceProjectiles);
        }
    }
}