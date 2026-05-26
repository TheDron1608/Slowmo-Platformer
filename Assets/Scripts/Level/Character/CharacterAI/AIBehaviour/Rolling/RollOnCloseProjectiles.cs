using UnityEngine;

public class RollOnCloseProjectiles : AbstractAIRolling
{
    public float RollCooldown = 1f;
    public float MeleeProjectileDistanceToRoll = 2.5f;
    public float RangedProjectileDistanceToRoll = 10f;

    private float _cooldown = 0f;

    private void FixedUpdate()
    {
        if (CharComponents.CharacterRolling.IsRolling) return;
        if (_cooldown > 0f)
        {
            _cooldown -= Time.fixedDeltaTime;
            return;
        }

        AbstractProjectile closestProjectile = null;
        float closestProjectileDistance = RangedProjectileDistanceToRoll;
        foreach (AbstractProjectile projectile in LayerManager.Instance.GetZLayerOfGameObject(gameObject).ProjectilesContainer.GetComponentsInChildren<AbstractProjectile>())
        {
            if ((!projectile.Owner?.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(CharComponents.CharacterTeam)) ?? true)
            {
                float projectileDistance = Vector2.Distance(
                   projectile.TryGetComponent(out RangedProjectile rangedProjectile) ? rangedProjectile.ProjectileTip.transform.position : projectile.transform.position,
                   CharComponents.Center.transform.position);
                if (projectileDistance < closestProjectileDistance)
                {
                    closestProjectile = projectile;
                    closestProjectileDistance = projectileDistance;
                }
            }
        }

        if (closestProjectile != null)
        {
            if (closestProjectile is RangedProjectile closestRangedProjectile && closestProjectileDistance <= RangedProjectileDistanceToRoll)
            {
                if (GetProjectileMovingToCharacter(closestRangedProjectile))
                {
                    CharComponents.CharacterRolling.TryRoll(
                        closestRangedProjectile.MoveAlignVec2.x > 0f ? 
                        (float)CharacterRolling.RollDirection.Left :
                        (float)CharacterRolling.RollDirection.Right
                        );
                }
            }
            else if (closestProjectile is MeleeProjectile closestMeleeProjectile && closestProjectileDistance <= MeleeProjectileDistanceToRoll)
            {
                CharComponents.CharacterRolling.TryRoll(
                    CharComponents.transform.position.x > closestMeleeProjectile.transform.position.x ?
                    (float)CharacterRolling.RollDirection.Right :
                    (float)CharacterRolling.RollDirection.Left
                    );
            }
        }
    }

    private bool GetProjectileMovingToCharacter(AbstractProjectile projectile)
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        RaycastHit2D rayCast = Physics2D.Raycast(
                projectile.transform.position,
                VectorMath.Quartenion2DToVec2(projectile.transform.rotation),
                projectile.ProjectileSize + RangedProjectileDistanceToRoll,
                (1 << layer.CharactersLayer) | (1 << layer.EnviromentLayer)
                );

        if (rayCast.collider.TryGetComponent(out AbstractCharacterComponent character))
        {
            return character.CharComponents == CharComponents;
        }
        else
        {
            return false;
        }
    }
}
