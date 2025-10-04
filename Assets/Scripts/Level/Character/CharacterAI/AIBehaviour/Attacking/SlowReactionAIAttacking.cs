using System.Collections;
using UnityEngine;

public class SlowReactionAIAttacking : AbstractAIAttacking
{
    public float StartAttackDelaySeconds = 1f;
    public float StopAimingDelaySeconds = 3.5f;
    public float MaxRangeForMeleeAttack = 1.75f;
    public bool AlwaysHammerWeaponBeforeAttack = true;

    private Vector2? _currentAttackPoint;
    private bool _attackIsDelaying = true;
    private Coroutine _attackDelayingCoroutine = null;

    private void FixedUpdate()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            //aiming at enemy
            if (_currentAttackPoint == null)
            {
                _currentAttackPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;
                CharComponents.CharacterAiming.TargetAimPoint = _currentAttackPoint.Value;
            }

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
                                (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) && !rangedWeapon.GetIsOutOfAmmo()) ||
                                (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<MeleeWeapon>() != null && _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance.Value <= MaxRangeForMeleeAttack)
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
                else if (CharComponents.CharacterAiming.GetCurrentAimReachedTargetAim())
                {
                    if (CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.TargetAimPoint))
                    {
                        _currentAttackPoint = null;
                    }
                }
            }
            else
            {
                _attackIsDelaying = true;
            }
        }
        else if (_selfStateBehaviourAI.NearestEnemyInfo.TimeSinceLastEnemyDetection > StopAimingDelaySeconds)
        {
            //stops hammering weapon if no enemy nearby
            CharComponents.CharacterAttacking.TryStopHammerringWeapon();
            _currentAttackPoint = null;
            CharComponents.CharacterAiming.AimWeaponDown = true;
            _attackIsDelaying = true;
        }
    }

    private IEnumerator AwaitStartAttackDelay()
    {
        yield return new WaitForSeconds(StartAttackDelaySeconds);
        _attackIsDelaying = false;
        _attackDelayingCoroutine = null;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterAttacking.OnAttack += CharacterAttacking_OnAttack;
    }

    private void CharacterAttacking_OnAttack(object sender, bool e)
    {
        _currentAttackPoint = null;
    }

    private void OnDestroy()
    {
        CharComponents.CharacterAttacking.OnAttack -= CharacterAttacking_OnAttack;
    }

    private void OnDisable()
    {
        if (_attackDelayingCoroutine != null)
        {
            StopCoroutine(_attackDelayingCoroutine);
            _attackDelayingCoroutine = null;
        }
    }
}
