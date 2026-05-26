using UnityEngine;

public abstract class AbstractRangedWeaponEffect : AbstractWeaponEffect, IRangedWeaponEffect
{
    private RangedWeapon _rangedWeapon;

    public RangedWeapon RangedWeapon
    {
        get => _rangedWeapon;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out RangedWeapon rw);
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _rangedWeapon)) throw new UnityException("rangedWeapon component not found at " + gameObject.name);
    }
}
