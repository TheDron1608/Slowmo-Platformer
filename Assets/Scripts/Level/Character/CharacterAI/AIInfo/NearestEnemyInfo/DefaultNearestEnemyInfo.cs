using UnityEngine;

public partial class DefaultNearestEnemyInfo : AbstractAINearestEnemyInfo
{
    protected override void OnUpdateInfo()
    {
        float minDistance = MaxEnemyDetectRange;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        CharacterTeam result = null;
        foreach (CharacterComponentsManager character in currentLayer.CharactersContainer.GetComponentsInChildren<CharacterComponentsManager>())
        {
            float charDistance = Vector2.Distance(transform.position, character.transform.position);
            if (
                charDistance < minDistance &&
                !character.CharacterEffectsReceiver.GetHasEffect<Death>() &&
                !NumberMath.GetListContainsAnyItemOfAnotherList(character.CharacterTeam.CharacterTeams, CharComponents.CharacterTeam.CharacterTeams) &&
                Physics2D.Linecast(
                    CharComponents.Center.transform.position,
                    character.Center.transform.position,
                    1 << currentLayer.EnviromentLayer
                    ).collider == null
                )
            {
                minDistance = charDistance;
                result = character.CharacterTeam;
            }
        }

        if (result != null)
        {
            _timeSinceLastEnemyDetection = 0f;
        }
        if (_nearestEnemy != null)
        {
            _lastEnemyPosition = _nearestEnemy.transform.position;
        }

        _nearestEnemy = result;
        _nearestEnemyDistance = minDistance;
    }
}
