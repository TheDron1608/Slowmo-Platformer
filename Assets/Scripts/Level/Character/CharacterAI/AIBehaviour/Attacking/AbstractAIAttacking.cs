public abstract class AbstractAIAttacking : AbstractAIBehaviour
{
    protected void AttackOrThrowGrenadeAtEnemy()
    {
        if (
            (CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out OnInteractArmGrenade grenade) ?? false)
            )
        {
            if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null)
            {
                CharComponents.CharacterAttacking.TryArmGrenade();
                CharComponents.CharacterHolding.TryThrow(
                    (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position - CharComponents.Center.transform.position).normalized
                    );
            }
        }
        else
        {
            CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
        }
    }
}
