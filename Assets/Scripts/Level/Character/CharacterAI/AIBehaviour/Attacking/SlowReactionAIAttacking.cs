using UnityEngine;

public class SlowReactionAIAttacking : AbstractAIAttacking
{
    const float MAX_RANGE_FOR_MELEE_ATTACK = 1.75f;

    private Vector2? _currentAttackPoint;

    private void FixedUpdate()
    {
        if (_currentAttackPoint != null) return;

        if (CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemy != null)
        {
            if (
                CharComponents.CharacterAttacking.IsAbleToAttack && 
                    (
                    CharComponents.CharacterAttacking.UnarmedAttackProjectile != null ||
                        (
                            CharComponents.CharacterHolding.CurrentHoldObject != null && 
                            (
                                (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) && !rangedWeapon.GetIsOutOfAmmo()) ||
                                (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<MeleeWeapon>() != null && CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemyDistance.Value <= MAX_RANGE_FOR_MELEE_ATTACK)
                            )
                        )
                    )
                )
            {
                _currentAttackPoint = CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;

                CharComponents.CharacterAiming.TargetAimPoint = _currentAttackPoint.Value;

                CharComponents.CharacterAttacking.TryAttack(_currentAttackPoint.Value);
            }
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterAttacking.OnAttack += CharacterAttacking_OnAttack;
    }

    private void CharacterAttacking_OnAttack(object sender, System.EventArgs e)
    {
        _currentAttackPoint = null;
    }

    private void OnDestroy()
    {
        CharComponents.CharacterAttacking.OnAttack -= CharacterAttacking_OnAttack;
    }
}
