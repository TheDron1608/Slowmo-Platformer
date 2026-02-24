using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponEffectsOnSpawnByTagModificator : AbstractMultiplierableModificator
{
    public Weapon.WEAPON_TAGS[] Tag;
    public List<AbstractEffect> Effects = new();

    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        base.OnObjectSpawned(sender, e);

        if (
            e.TryGetComponent(out Weapon weapon) &&
            e.TryGetComponent(out Holdable holdableWeapon) &&
            e.TryGetComponent(out ObjectEffectsReceiver weaponEffectReceiver) &&
            (Tag.Length == 0 || Tag.All(tag => weapon.Tags.Contains(tag)))
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
                        holdableTransform.TryGetComponent(out Holdable holdableWeapon) &&
                        holdableTransform.TryGetComponent(out ObjectEffectsReceiver weaponEffectReceiver) &&
                        (Tag.Length == 0 || Tag.All(tag => weapon.Tags.Contains(tag)))
                        )
                    {
                        weaponEffectReceiver.RemoveEffect(Effects);
                    }
                }
            }
        }
    }
}