using UnityEngine;
using UnityEngine.TextCore.Text;

public class TeleportToTargetIfCantReach : AbstractAISpecial
{
    public float MaxDistanceToTarget = 5f;
    public float UnableToReachAwaitTime = 3f;

    private float _cantReachTimeSpent = 0f;

    private void FixedUpdate()
    {
        if (
            (CharComponents.CharacterSpecial?.TryGetComponent(out CharacterBleedTeleportation bleedTeleporatation) ?? false) &&
            !bleedTeleporatation.IsTeleporting &&
            _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null &&
            !_selfStateBehaviourAI.Pathfinding.GetIsAbleToReachPathTarget() &&
            Physics2D.Linecast(
                CharComponents.Center.transform.position,
                _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position,
                1 << CharComponents.CharacterCollision.CurrentZLayer.EnviromentLayer
                ).collider != null
            )
        {
            _cantReachTimeSpent += Time.fixedDeltaTime;

            if (_cantReachTimeSpent > UnableToReachAwaitTime)
            {
                CharacterComponentsManager closesetCharacter = null;
                float closestCharacterDistance = MaxDistanceToTarget;
                foreach (Transform characterTrasnform in CharComponents.CharacterCollision.CurrentZLayer.CharactersContainer)
                {
                    if (
                        characterTrasnform.gameObject.activeSelf && 
                        characterTrasnform.TryGetComponent(out CharacterComponentsManager character) &&
                        character.CharacterTeam.Team != _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.Team &&
                        !CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharacterTeam)
                        )
                    {
                        float distance = Vector2.Distance(
                            _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position, 
                            characterTrasnform.position
                            );

                        if (distance < closestCharacterDistance)
                        {
                            closestCharacterDistance = distance;
                            closesetCharacter = character;
                        }
                    }
                }

                if (closesetCharacter != null && (CharComponents.CharacterSpecial?.TryGetComponent(out CharacterBleedTeleportation bleedTele) ?? false))
                {
                    bleedTele.TryTeleport(closesetCharacter);
                }

                _cantReachTimeSpent = 0f;
            }
        }
        else
        {
            _cantReachTimeSpent = 0f;
        }
    }
}
