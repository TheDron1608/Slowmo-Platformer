using System.Collections;
using UnityEngine;

public class DefaultAIAttacking : AbstractAIAttacking
{
    public float StartAttackDelaySeconds = 1f;
    public float StopAttackAimingDelaySeconds = 3.5f;
    public bool AlwaysHammerWeaponBeforeAttack = true;

    private bool _attackIsDelaying = true;
    private Coroutine _attackDelayingCoroutine = null;

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
                CharComponents.CharacterAiming.AimWeaponDown = false;
                if (CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<RangedWeapon>() != null && _attackIsDelaying)
                {
                    if (_attackDelayingCoroutine == null) _attackDelayingCoroutine = StartCoroutine(AwaitStartAttackDelay());
                }
                else
                {
                    CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
                }
            }
        }
        else if (_selfStateBehaviourAI.NearestEnemyInfo.TimeSinceLastEnemyDetection > StopAttackAimingDelaySeconds)
        {
            _attackIsDelaying = true;
            CharComponents.CharacterAttacking.TryStopHammerringWeapon();
            CharComponents.CharacterAiming.AimWeaponDown = true;
        }
    }

    private IEnumerator AwaitStartAttackDelay()
    {
        yield return new WaitForSeconds(StartAttackDelaySeconds);
        _attackIsDelaying = false;
        _attackDelayingCoroutine = null;
    }
}
