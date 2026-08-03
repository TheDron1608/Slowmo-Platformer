
public class KillSelfOnNearEnemy : AbstractAISpecial
{
    public float DistanceToKillSelf;

    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterSpecial != null &&
            CharComponents.CharacterSpecial.TryGetComponent(out ApplySelfEffects suicide) &&
            _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null &&
            _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance < DistanceToKillSelf
            )
        {
            suicide.TryApplySelfEffects();
        }
    }
}