using UnityEngine;

public class GoTowardAllies : AbstractAIPathfindingMovingAndJumping
{
    public float AllyDetectDistance = 5f;
    public float TowardExtraDistance = 1.25f;

    protected override void UpdatePathTarget()
    {
        if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy == null) return;

        float closestToEnemyAllyDistance = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance.Value;
        CharacterTeam closestToEnemyAlly = null;

        foreach (CharacterTeam ally in TeamManager.Instance.GetTeamDataByTeam(CharComponents.CharacterTeam.Team).GetTeamMembers())
        {
            if (
                ally.CharComponents.CharacterCollision.CurrentZLayer == CharComponents.CharacterCollision.CurrentZLayer &&
                ally != CharComponents.CharacterTeam &&
                Vector2.Distance(ally.CharComponents.Center.transform.position, CharComponents.Center.transform.position) < AllyDetectDistance
                )
            {
                float allyDistanceToEnemy = Vector2.Distance(
                    ally.CharComponents.Center.transform.position,
                    _selfStateBehaviourAI.NearestEnemyInfo.CharComponents.Center.transform.position
                    );

                if (allyDistanceToEnemy < closestToEnemyAllyDistance)
                {
                    closestToEnemyAllyDistance = allyDistanceToEnemy;
                    closestToEnemyAlly = ally;
                }
            }
        }

        if (closestToEnemyAlly != null)
        {
            Vector2 extraOffset = 
                (
                    _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position - 
                    CharComponents.Center.transform.position
                ).normalized * TowardExtraDistance;
            
            _selfStateBehaviourAI.Pathfinding.PathTarget = new(
                closestToEnemyAlly.transform.position + VectorMath.Vec2ToVec3(extraOffset), 
                closestToEnemyAlly.CharComponents.CharacterCollision.CurrentZLayer
                );
        }
        else
        {
            _selfStateBehaviourAI.Pathfinding.PathTarget = null;
        }
    }
}
