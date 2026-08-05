
public class AttackCooldownMultiplier : AbstractWeaponEffect
{
    public float CooldownMult = 1f;

    protected override void OnApply()
    {
        base.OnApply();

        Weapon.AttackCooldownMultiplier *= CooldownMult;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        Weapon.AttackCooldownMultiplier /= CooldownMult;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && CooldownMult == (other as AttackCooldownMultiplier).CooldownMult;
    }
}