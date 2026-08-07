using UnityEngine;

public abstract class AbstractDelayedAttacking : AbstractAIAttacking
{
    protected const float CLUMSY_MELEE_ATTACK_MIN_DELAY = 0.5f;

    public float RangedAttackDelaySeconds = 0.75f;
    public float MeleeAttackDelaySeconds = 0.25f;
    public float StopAttackAimingDelaySeconds = 3.5f;
    public bool AlwaysHammerWeaponBeforeAttack = true;
    public bool DoNotAimIfNeedToMove = false;
    public bool AllowArms = true;
    public bool AllowUnarmed = true;

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {
        if (
            DoNotAimIfNeedToMove && 
            CharComponents.CharacterClumsyness.GetIsClumsyAttackWithCurrentWeapon() &&
            _selfStateBehaviourAI.Pathfinding.PathTarget != null &&
            _selfStateBehaviourAI.Pathfinding.PathTarget.Value.Position != TileManager.PositionToTilePosition(transform.position)
            )
        {
            OnLostEnemy();
            return;
        }

        Weapon currentWeapon =
            (AllowArms ? CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<Weapon>() : null) ??
            (AllowUnarmed ? CharComponents.UnarmedAttacking : null);

        if (currentWeapon != null && _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            CharComponents.CharacterAiming.TargetAimPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;

            //trying hammer weapon if AlwayHammerWeaponBeforeAttack else attack immediantely
            if (
                AlwaysHammerWeaponBeforeAttack &&
                currentWeapon.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) &&
                !hammerWeapon.Hammered
                )
            {
                CharComponents.CharacterAttacking.TryHammerWeapon();
            }

            //trying start chainsaw
            else if (
                currentWeapon.TryGetComponent(out Chainsaw chainsaw) &&
                !chainsaw.Started
                )
            {
                CharComponents.CharacterAttacking.TryStartChainsaw();
            }

            //trying attack if no need to hammer weapon or start chainsaw
            else if (
                (
                    CharComponents.CharacterAttacking.IsAbleToAttack &&
                    currentWeapon.Projectile != null &&
                    currentWeapon.GetIsAbleToAttack() &&
                    (
                        currentWeapon.Projectile is RangedProjectile
                    ) ||
                    (
                         currentWeapon.Projectile is MeleeProjectile meleeProjectile &&
                        _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance.Value <= meleeProjectile.ProjectileSize
                    )
                ) ||
                (
                    CharComponents.CharacterAttacking.IsAbleToAttack &&
                    CharComponents.CharacterHolding.IsAbleToThrowObjects &&
                    (CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out OnInteractArmGrenade grenade) ?? false) &&
                    ((Physics2D.Linecast(
                        CharComponents.CharacterHolding.CurrentHoldObject.transform.position,
                        _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position,
                        1 << CharComponents.CharacterCollision.CurrentZLayer.CharactersLayer
                        ).collider?.TryGetComponent(out AbstractCharacterComponent character) ?? false) ? 
                        character.CharComponents == _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents : false)
                )
                )
            {
                OnTrackedEnemy();
            }
            else
            {
                OnLostEnemy();
            }
        }
        else if (_selfStateBehaviourAI.NearestEnemyInfo.TimeSinceLastEnemyDetection > StopAttackAimingDelaySeconds)
        {
            CharComponents.CharacterAttacking.TryStopAttack();
            CharComponents.CharacterAiming.AimWeaponDown = true;
        }
    }

    protected abstract void OnTrackedEnemy();
    protected abstract void OnLostEnemy();
}
