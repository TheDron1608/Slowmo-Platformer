public class GoToNearestValidHoldable : AbstractAIPathfindingMovingAndJumping
{
    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null)
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = new(_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.transform.position, gameObject);
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}
