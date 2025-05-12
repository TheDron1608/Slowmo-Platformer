using UnityEngine;

public class SlowReactionAIAttacking : AbstractAIAttacking
{
    public float MaxRangeForMeleeAttack = 1.75f;
    public bool AlwaysHammerWeaponBeforeAttack = true;

    private Vector2? _currentAttackPoint;

    private void FixedUpdate()
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
                                (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) && !rangedWeapon.GetIsOutOfAmmo()) ||
                                (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<MeleeWeapon>() != null && _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance.Value <= MaxRangeForMeleeAttack)
                            )
                        )
                    )
                )
            {
                CharComponents.CharacterAiming.TargetAimPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;
                if (CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.TargetAimPoint))
                {
                    _currentAttackPoint = CharComponents.CharacterAiming.TargetAimPoint;
                }

            }
        }
        else
        {
            //stops hammering weapon if no enemy nearby
            CharComponents.CharacterAttacking.TryStopHammerringWeapon();
        }
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
}
