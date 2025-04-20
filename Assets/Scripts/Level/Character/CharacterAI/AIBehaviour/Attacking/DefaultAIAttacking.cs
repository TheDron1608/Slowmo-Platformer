using UnityEngine;

public class DefaultAIAttacking : AbstractAIAttacking
{
    public float MaxRangeForMeleeAttack = 1.75f;

    private void FixedUpdate()
    {
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
                                (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<MeleeWeapon>() != null && CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemyDistance.Value <= MaxRangeForMeleeAttack)
                            )
                        )
                    )
                )
            {
                CharComponents.CharacterAiming.TargetAimPoint = CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;

                CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
            }
        }
    }
}
