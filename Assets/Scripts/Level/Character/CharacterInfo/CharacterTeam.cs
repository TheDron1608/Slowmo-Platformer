using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTeam : AbstractCharacterComponent
{
    const float UPDATE_NEAREST_AVAIBLE_ENEMY_DELAY_SECONDS = 0.34f;

    public enum Teams
    {
        PLAYER,
        DEFAULT_ENEMY
    }

    public List<Teams> CharacterTeams = new();
    private CharacterTeam _currentNearestAvaibleEnemy = null;
    private bool _updatedNearestAvaibleEnemyCharacterThisFixedUpdate = false;

    public bool GetIsInTeam(Teams team)
    {
        return CharacterTeams.Contains(team);
    }

    public CharacterTeam GetNearestEnemyCharacter()
    {
        if (!_updatedNearestAvaibleEnemyCharacterThisFixedUpdate)
        {
            UpdateNearestEnemyTeamCharacter();
        }
        return _currentNearestAvaibleEnemy;
    }

    private void UpdateNearestEnemyTeamCharacter()
    {
        float minDistance = 99999f;
        CharacterTeam result = null;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        foreach (Transform characterGameObject in currentLayer.CharactersContainer)
        {
            if (characterGameObject.TryGetComponent(out CharacterTeam characterTeam) && !NumberMath.GetListContainsAnyItemOfAnotherList(characterTeam.CharacterTeams, this.CharacterTeams))
            {
                float charDistance = Vector2.Distance(transform.position, characterGameObject.transform.position);
                if (
                    charDistance < minDistance && 
                    Physics2D.Linecast(
                        CharComponents.Center.transform.position, 
                        characterTeam.CharComponents.Center.transform.position, 
                        1 << currentLayer.EnviromentLayer
                        ).collider == null
                    )
                {
                    minDistance = charDistance;
                    result = characterTeam;
                }
            }
        }
        _currentNearestAvaibleEnemy = result;
        _updatedNearestAvaibleEnemyCharacterThisFixedUpdate = true;
    }

    private void FixedUpdate()
    {
        _updatedNearestAvaibleEnemyCharacterThisFixedUpdate = false;
    }
}