using UnityEngine;

public class DefaultAIAttacking : AbstractAIAttacking
{
    const float MAX_AIM_POINT_DISTANCE_TO_TARGET_TO_ATTACK = 1f;

    public bool AlwaysHammerWeaponBeforeAttack = true;

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            //aiming at enemy
            CharComponents.CharacterAiming.TargetAimPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;

            //trying hammer weapon if AlwayHammerWeaponBeforeAttack else attack immediantely
            if (
                AlwaysHammerWeaponBeforeAttack &&
                CharComponents.CharacterHolding.CurrentHoldObject != null &&
                CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && 
                !hammerWeapon.Hammered
                )
            {
                CharComponents.CharacterAttacking.TryHammerWeapon();
            }

            //trying start chainsaw
            else if (
                CharComponents.CharacterHolding.CurrentHoldObject != null &&
                CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Chainsaw chainsaw) && 
                !chainsaw.Started
                )
            {
                CharComponents.CharacterAttacking.TryStartChainsaw();
            }

            //trying attack if no need to hammer weapon or start chainsaw
            else if (
                CharComponents.CharacterAttacking.IsAbleToAttack && 
                Vector2.Distance(CharComponents.CharacterAiming.CurrentAimPoint, CharComponents.CharacterAiming.TargetAimPoint) <= MAX_AIM_POINT_DISTANCE_TO_TARGET_TO_ATTACK &&
                    (
                    CharComponents.CharacterAttacking.UnarmedAttackProjectile != null ||
                        (
                            CharComponents.CharacterHolding.CurrentHoldObject != null && 
                            (
                                (
                                    CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) && 
                                    rangedWeapon.Projectile != null &&
                                    !rangedWeapon.GetIsOutOfAmmo()
                                ) ||
                                (
                                    CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out MeleeWeapon meleeWeapon) && 
                                    meleeWeapon.Projectile != null &&
                                    _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance.Value <= meleeWeapon.Projectile.ProjectileSize
                                )
                            )
                        )
                    )
                )
            {
                CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
            }
        }
        else
        {
            //stops hammering weapon if no enemy nearby
            CharComponents.CharacterAttacking.TryStopHammerringWeapon();
        }
    }
}
