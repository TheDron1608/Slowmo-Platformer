using UnityEngine;

public class RandomAttackingOnUnholded : AbstractWeaponEffect, IMultiplierableEffect
{
    public float ChancePerSecond = 0.125f;
    public RandomManager.ProcChanceTypes ChanceType = RandomManager.ProcChanceTypes.BAD;

    private Holdable _affectedWeaponHoldableComponent;
    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        _affectedWeaponHoldableComponent = Weapon.GetComponent<Holdable>();
    }

    private void FixedUpdate()
    {
        if (
            _affectedWeaponHoldableComponent.CurrentHolder == null && 
            RandomManager.Instance.ProcRandomChance(ChancePerSecond * EffectMultiplier * Time.fixedDeltaTime, ChanceType)
            )
        {
            Weapon.TryAttack(VectorMath.Quartenion2DToVec2(Weapon.transform.rotation));
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.GetComponent<Holdable>() != null;
    }
}
