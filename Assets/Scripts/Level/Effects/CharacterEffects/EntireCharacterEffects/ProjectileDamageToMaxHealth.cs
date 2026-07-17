using UnityEngine;

public class ProjectileDamageToMaxHealth : AbstractDamagableObjectEffect
{
    public float HealthConversion = 0.5f;

    private float _addedHealth = 0f;

    protected override void OnApply()
    {
        base.OnApply();

        AffectedDamagableObject.OnHitByProjectile += AffectedDamagableObject_OnHitByProjectile;
    }

    private void AffectedDamagableObject_OnHitByProjectile(object sender, AbstractProjectile e)
    {
        Damage dmgEffect = e.HitEffects.Find(e => e is Damage) as Damage;
        if (dmgEffect != null && AffectedObject.ApplyCondition(dmgEffect, e))
        {
            _addedHealth += dmgEffect.DamageAmount;
            AffectedDamagableObject.ApplyMaxHealth(AffectedDamagableObject.MaxHealth + dmgEffect.DamageAmount, null);
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedDamagableObject.OnHitByProjectile -= AffectedDamagableObject_OnHitByProjectile;
        AffectedDamagableObject.ApplyMaxHealth(AffectedDamagableObject.MaxHealth - _addedHealth, null);
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && HealthConversion == (other as ProjectileDamageToMaxHealth).HealthConversion;
    }
}
