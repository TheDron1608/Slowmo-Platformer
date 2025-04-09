using UnityEngine;

public class DefaultAIAiming : AbstractAIAiming
{
    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterAttacking.IsAbleToAttack && 
                (
                CharComponents.CharacterAttacking.UnarmedAttackProjectile != null ||
                (CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<Weapon>() != null)
                )
            )
        {
            if (CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemy != null)
            {
                CharComponents.CharacterAiming.TargetAimPoint = CharComponents.CharacterAIManager.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;

                CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
            }
        }
    }
}
