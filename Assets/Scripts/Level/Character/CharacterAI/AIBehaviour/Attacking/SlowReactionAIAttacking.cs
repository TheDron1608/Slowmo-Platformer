using UnityEngine;

public class SlowReactionAIAttacking : AbstractAIAttacking
{
    public float MaxRangeForMeleeAttack = 1.75f;

    private Vector2? _currentAttackPoint;

    private void FixedUpdate()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
        {
            if (
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
