using UnityEngine;

public abstract class AbstractMeleeWeaponEffectWithSender : AbstractWeaponEffectWithSender, IMeleeWeaponEffect
{
    private MeleeWeapon _meleeWeapon;

    public MeleeWeapon MeleeWeapon
    {
        get => _meleeWeapon;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<MeleeWeapon>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _meleeWeapon)) throw new UnityException("MeleeWeapon component not found at " + gameObject.name);
    }
}
