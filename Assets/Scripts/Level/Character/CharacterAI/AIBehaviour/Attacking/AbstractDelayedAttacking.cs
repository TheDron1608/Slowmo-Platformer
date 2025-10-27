using System.Collections;
using UnityEngine;

public abstract class AbstractDelayedAttacking : AbstractAIAttacking
{
    public float RangedAttackDelaySeconds = 0.75f;
    public float MeleeAttackDelaySeconds = 0.25f;
    public float StopAttackAimingDelaySeconds = 3.5f;
    public bool AlwaysHammerWeaponBeforeAttack = true;

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
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
                OnTrackedEnemy();
            }
            else
            {
                OnLostEnemy();
            }
        }
        else if (_selfStateBehaviourAI.NearestEnemyInfo.TimeSinceLastEnemyDetection > StopAttackAimingDelaySeconds)
        {
            CharComponents.CharacterAttacking.TryStopHammerringWeapon();
            CharComponents.CharacterAiming.AimWeaponDown = true;
        }
    }

    protected abstract void OnTrackedEnemy();
    protected abstract void OnLostEnemy();
}
