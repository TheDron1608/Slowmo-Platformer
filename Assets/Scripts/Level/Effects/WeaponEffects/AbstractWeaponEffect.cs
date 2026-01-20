using UnityEngine;

public abstract class AbstractWeaponEffect : AbstractEffect, IWeaponEffect
{
    private Weapon _weapon;

    public Weapon Weapon
    {
        get => _weapon;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<Weapon>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _weapon)) throw new UnityException("Weapon component not found at " + gameObject.name);
    }
}
