using UnityEngine;

public class DefaultAIAiming : AbstractAI
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
            if (CharComponents.CharacterTeam.GetNearestEnemyCharacter() != null)
            {
                CharComponents.CharacterAiming.TargetAimPoint = CharComponents.CharacterTeam.GetNearestEnemyCharacter().CharComponents.Center.transform.position;

                CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
            }
        }
    }
}
