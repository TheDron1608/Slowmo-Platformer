using UnityEngine;

public class MeleeWeapon : ThrowableWeapon
{
    [Header("Melee weapon")]
    public float AttackRangeMultiplier = 1f;

    private BreakableHoldable _breakableHoldableComponent;

    protected override void OnAwake()
    {
        base.OnAwake();

        TryGetComponent(out _breakableHoldableComponent);
    }

    public override string GetAmmoInfoOnSelect()
    {
        if (_breakableHoldableComponent == null || _breakableHoldableComponent.UnlimitedUses)
        {
            return "";
        }
        else
        {
            return _breakableHoldableComponent.UsesLeft + " / " + _breakableHoldableComponent.MaxUses;
        }
    }
}
