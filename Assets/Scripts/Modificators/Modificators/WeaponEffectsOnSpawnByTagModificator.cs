using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponEffectsOnSpawnByTagModificator : AbstractModificator
{
    public Weapon.WEAPON_TAGS[] Tag;
    public List<AbstractEffect> Effects = new();

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                foreach (Transform holdableT in layer.HoldablesContainer)
                {
                    if (
                        holdableT.TryGetComponent(out Weapon weapon) &&
                        (Tag.Length == 0 || Tag.All(tag => weapon.Tags.Contains(tag))) &&
                        holdableT.TryGetComponent(out Holdable holdableWeapon) &&
                        holdableT.TryGetComponent(out ObjectEffectsReceiver weaponEffectReceiver)
                        )
                    {
                        weaponEffectReceiver.ApplyEffect(Effects, null, ModificatorMultiplier, true);
                    }
                }
            }
        }
    }

    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        base.OnObjectSpawned(sender, e);

        if (
            e.TryGetComponent(out Weapon weapon) &&
            (Tag.Length == 0 || Tag.All(tag => weapon.Tags.Contains(tag))) &&
            e.TryGetComponent(out Holdable holdableWeapon) &&
            e.TryGetComponent(out ObjectEffectsReceiver weaponEffectReceiver)
            ) 
        {
            weaponEffectReceiver.ApplyEffect(Effects, null, ModificatorMultiplier, true);
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                foreach (Transform holdableTransform in layer.HoldablesContainer)
                {
                    if (
                        holdableTransform.TryGetComponent(out Weapon weapon) &&
                        (Tag.Length == 0 || Tag.All(tag => weapon.Tags.Contains(tag))) &&
                        holdableTransform.TryGetComponent(out Holdable holdableWeapon) &&
                        holdableTransform.TryGetComponent(out ObjectEffectsReceiver weaponEffectReceiver) 
                        )
                    {
                        weaponEffectReceiver.RemoveEffect(Effects);
                    }
                }
            }
        }
    }
}