public class Shielding : AbstractAIAttacking 
{
    public float MinDistance = 7.5f;

    private void FixedUpdate()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null && _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance < MinDistance)
        {
            CharComponents.CharacterAiming.AimWeaponDown = false;
            CharComponents.CharacterAiming.TargetAimPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;
            CharComponents.CharacterAttacking.TryShield();
        }
        else
        {
            CharComponents.CharacterAiming.AimWeaponDown = true;
            CharComponents.CharacterAttacking.TryStopShield();
        }
    }
}
