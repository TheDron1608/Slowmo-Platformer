using UnityEngine;

public class ChaseForeverAI : AbstractAIPathfindingMovingAndJumping
{
    public GameObject ChaseWho;

    protected override void UpdatePathTarget()
    {
        if (ChaseWho != null)
        {
            CharComponents.CharacterAIManager.AIPathfinding.PathTarget = ChaseWho.transform.position;
        }
        else
        {
            CharComponents.CharacterAIManager.AIPathfinding.PathTarget = null;
        }
    }
}
