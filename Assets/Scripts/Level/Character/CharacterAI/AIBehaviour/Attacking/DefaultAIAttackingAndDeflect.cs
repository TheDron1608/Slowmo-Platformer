using UnityEngine;

public class DefaultAIAttackingAndDeflect : DefaultAIAttacking
{
    const float RANGED_PROJECTILE_DEFLECTION_SPEED_DELAY = 1f;
    const float MELEE_PROJECTILE_DEFLECTION_DETECTION_EXTRA_DISTANCE = 1.5f;
    const float PROJECTILE_DEFLECTION_MAX_DISTANCE = 15f;
    const float DEFLECT_MAX_AXIS = 1f;

    public bool InstantDeflect = true;

    protected override void OnFixedUpdate()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        AbstractProjectile[] projectiles = layer.ProjectilesContainer.GetComponentsInChildren<AbstractProjectile>();
        MeleeProjectile currentProjectile = null;
        if (
            CharComponents.CharacterAttacking.IsAbleToAttack &&
            (
                (
                    (CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out MeleeWeapon meleeW) ?? false) &&
                    (meleeW.Projectile?.TryGetComponent(out currentProjectile) ?? false)
                ) ||
                (CharComponents.UnarmedAttacking.Projectile?.TryGetComponent(out currentProjectile) ?? false)
                )
            )
        {
            AbstractProjectile closestProjectile = null;
            float closestProjectileDistance = PROJECTILE_DEFLECTION_MAX_DISTANCE;
            foreach (AbstractProjectile projectile in projectiles)
            {
                if (
                    ((!projectile.Owner?.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(CharComponents.CharacterTeam)) ?? true) &&
                    GetProjectileIsValidToDeflect(currentProjectile, projectile)
                    )
                {
                    float projectileDistance = Vector2.Distance(
                        projectile.transform.position, 
                        CharComponents.Center.transform.position + VectorMath.Vec2ToVec3(CharComponents.CharacterRigidBody.linearVelocity * RANGED_PROJECTILE_DEFLECTION_SPEED_DELAY));
                    if (projectileDistance < closestProjectileDistance)
                    {
                        closestProjectile = projectile;
                        closestProjectileDistance = projectileDistance;
                    }
                }
            }

            if (closestProjectile != null)
            {
                CharComponents.CharacterAiming.AimWeaponDown = false;
                CharComponents.CharacterAiming.TargetAimPoint = closestProjectile.transform.position;

                if (closestProjectile.TryGetComponent(out RangedProjectile rangedProjectile))
                {
                    if (
                        closestProjectileDistance <= currentProjectile.ProjectileSize + closestProjectile.ProjectileSize + rangedProjectile.BulletSpeed * RANGED_PROJECTILE_DEFLECTION_SPEED_DELAY
                        )
                    {
                        if (InstantDeflect)
                        {
                            CharComponents.CharacterAiming.InstantMoveToTargetAim();
                        }
                        if (CharComponents.CharacterAiming.GetCurrentAimReachedTargetAim(DEFLECT_MAX_AXIS))
                        {
                            CharComponents.CharacterAttacking.TryAttack((closestProjectile.transform.position - CharComponents.Center.transform.position).normalized);
                        }
                    }
                    return;
                }
                else if (
                    closestProjectile.TryGetComponent(out MeleeProjectile mp) &&
                    closestProjectileDistance <= currentProjectile.ProjectileSize + closestProjectile.ProjectileSize + MELEE_PROJECTILE_DEFLECTION_DETECTION_EXTRA_DISTANCE
                    )
                {
                    if (InstantDeflect)
                    {
                        CharComponents.CharacterAiming.InstantMoveToTargetAim();
                    }
                    if (CharComponents.CharacterAiming.GetCurrentAimReachedTargetAim(DEFLECT_MAX_AXIS))
                    {
                        CharComponents.CharacterAttacking.TryAttack((closestProjectile.transform.position - CharComponents.Center.transform.position).normalized);
                    }
                    return;
                }
            }
        }

        //if no need to deflect projectiles try attack enemy
        base.OnFixedUpdate();
    }

    private bool GetProjectileIsValidToDeflect(MeleeProjectile deflector, AbstractProjectile deflected)
    {
        return
            (
                deflected.TryGetComponent(out RangedProjectile rp) &&
                NumberMath.GetListContainsComponent<AbstractRangedProjectileDeflection, AbstractEffect>(deflector.EffectsOnDeflect)
            ) ||
            (
                deflected.TryGetComponent(out MeleeProjectile mp) &&
                NumberMath.GetListContainsComponent<AbstractMeleeProjectileDeflection, AbstractEffect>(deflector.EffectsOnDeflect)
            );
    }
}
