public class DoNotMove : AbstractAIMovingAndJumping
{
    private void FixedUpdate()
    {
        _selfStateBehaviourAI.Pathfinding.PathTarget = null;
    }
}
