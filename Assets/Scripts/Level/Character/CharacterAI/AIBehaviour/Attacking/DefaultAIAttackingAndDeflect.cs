using UnityEngine;

public class DefaultAIAttackingAndDeflect : DefaultAIAttacking
{
    const float RANGED_PROJECTILE_DEFLECTION_DETECTION_EXTRA_DISTANCE = 1.5f;
    const float MELEE_PROJECTILE_DEFLECTION_DETECTION_EXTRA_DISTANCE = 0.5f;
    const float PROJECTILE_DEFLECTION_PREPARE_DETECTION_EXTRA_DISTANCE = 10f;

    protected override void OnFixedUpdate()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        AbstractProjectile[] projectiles = layer.ProjectilesContainer.GetComponentsInChildren<AbstractProjectile>();
        MeleeProjectile currentProjectile =
            CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<MeleeWeapon>()?.Projectile?.GetComponent<MeleeProjectile>() ??
            CharComponents.CharacterAttacking.UnarmedAttackProjectile?.GetComponent<MeleeProjectile>();

        if (currentProjectile != null && CharComponents.CharacterAttacking.IsAbleToAttack)
        {
            foreach (AbstractProjectile projectile in projectiles)
            {
                if (
                    ((!projectile.Owner?.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(CharComponents.CharacterTeam)) ?? true) &&
                    GetProjectileIsValidToDeflect(currentProjectile, projectile)
                    )
                {
                    float distanceToProjectile = Vector2.Distance(CharComponents.Center.transform.position, projectile.transform.position);
                    if (
                        projectile.GetComponent<RangedProjectile>() != null &&
                        distanceToProjectile <= currentProjectile.ProjectileSize + PROJECTILE_DEFLECTION_PREPARE_DETECTION_EXTRA_DISTANCE &&
                        GetProjectileMovingToCharacter(projectile)
                        )
                    {
                        if (distanceToProjectile <= currentProjectile.ProjectileSize + RANGED_PROJECTILE_DEFLECTION_DETECTION_EXTRA_DISTANCE)
                        {
                            CharComponents.CharacterAttacking.TryAttack(projectile.transform.position);
                            return;
                        }
                        else
                        {
                            CharComponents.CharacterAiming.TargetAimPoint = projectile.transform.position;
                            return;
                        }
                    }
                    else if (
                        projectile.GetComponent<MeleeProjectile>() != null &&
                        distanceToProjectile <= currentProjectile.ProjectileSize + projectile.ProjectileSize + MELEE_PROJECTILE_DEFLECTION_DETECTION_EXTRA_DISTANCE
                        )
                    {
                        CharComponents.CharacterAttacking.TryAttack(projectile.transform.position);
                        return;
                    }
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
                deflected.GetComponent<RangedProjectile>() != null &&
                NumberMath.GetListContainsComponent<AbstractRangedProjectileDeflection, AbstractEffect>(deflector.EffectsOnDeflect)
            ) ||
            (
                deflected.GetComponent<MeleeProjectile>() != null &&
                NumberMath.GetListContainsComponent<AbstractMeleeProjectileDeflection, AbstractEffect>(deflector.EffectsOnDeflect)
            );
    }

    private bool GetProjectileMovingToCharacter(AbstractProjectile projectile)
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        return
            Physics2D.Raycast(
                projectile.transform.position,
                VectorMath.Quartenion2DToVec2(projectile.transform.rotation),
                projectile.ProjectileSize + PROJECTILE_DEFLECTION_PREPARE_DETECTION_EXTRA_DISTANCE,
                (1 << layer.CharactersLayer) | (1 << layer.EnviromentLayer)
                ).collider?.GetComponent<AbstractCharacterComponent>()?.CharComponents == CharComponents;
    }
}
