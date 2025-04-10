using UnityEngine;

public class DefaultAIAiming : AbstractAIAiming
{
    const float MAX_RANGE_FOR_MELEE_ATTACK = 1.75f;

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
                                CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<RangedWeapon>() != null ||
                                (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<MeleeWeapon>() != null && CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemyDistance.Value <= MAX_RANGE_FOR_MELEE_ATTACK)
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
