
using Unity.VisualScripting;

public class FollowBoss : AbstractAIPathfindingMovingAndJumping
{
    protected override void UpdatePathTarget()
    {
        if (BossInitializer.Instance?.Boss != null && !BossInitializer.Instance.Boss.IsDestroyed())
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = new(BossInitializer.Instance.Boss.transform.position, gameObject);
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}