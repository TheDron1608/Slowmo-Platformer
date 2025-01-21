using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Melee weapon")]
    public float AttackRangeMultiplier = 1f;
    public Projectile Projectile;

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        base.OnTryAttackSuccess(direction);

        Projectile projectile = Instantiate(Projectile, transform);

        if (TryGetComponent(out Holdable holdable) && holdable.CurrentHolder.TryGetComponent(out CharacterAiming characterAiming))
        {
            projectile.transform.LookAt(characterAiming.CurrentAimPoint);
            projectile.transform.rotation = VectorMath.Vec2ToQuarterninon2D(characterAiming.GetCurrentAimNormalized());
        }

        return true;
    }
}
