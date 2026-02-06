using System.Linq;
using UnityEngine;

public class OnAbleToDefendBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public float AllyDetectDistance = 5f;

    public override bool StateBehaviourCondition()
    {
        return
            CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<Shield>() != null &&
            CharComponents.CharacterAttacking.IsAbleToShield &&
            TeamManager.Instance.GetTeamDataByTeam(CharComponents.CharacterTeam.Team).GetTeamMembers().Any(GetAllyIsValidForDefend);
            
    }

    private bool GetAllyIsValidForDefend(CharacterTeam ally)
    {
        return
            ally.CharComponents.CharacterCollision.CurrentZLayer == CharComponents.CharacterCollision.CurrentZLayer &&
            ally != CharComponents.CharacterTeam &&
            Vector2.Distance(ally.CharComponents.Center.transform.position, CharComponents.Center.transform.position) < AllyDetectDistance &&
            !ally.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>();
    }
}
