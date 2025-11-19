using UnityEngine;

public partial class DefaultNearestEnemyInfo : AbstractAINearestEnemyInfo
{
    protected override void OnUpdateInfo()
    {
        float minDistance = MaxEnemyDetectRange;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        CharacterTeam result = null;
        foreach (Transform characterTransform in currentLayer.CharactersContainer.transform)
        {
            if (!characterTransform.gameObject.activeSelf) continue;
            if (!characterTransform.TryGetComponent(out CharacterComponentsManager character)) continue;

            float charDistance = Vector2.Distance(transform.position, character.transform.position);
            if (
                charDistance < minDistance &&
                !character.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() &&
                !CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharacterTeam) &&
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
            _lastEnemy = _nearestEnemy;
            _lastEnemyPosition = _nearestEnemy.transform.position;
            _lastEnemyLayer = LayerManager.Instance.GetZLayerOfGameObject(_nearestEnemy.gameObject);
        }

        _nearestEnemy = result;
        _nearestEnemyDistance = minDistance;

        if (_nearestEnemy == null && (_lastEnemy?.CharComponents.CharacterInteract.LastInteractObject?.TryGetComponent(out OnInteractEnterMultiZDoor zDoor) ?? false))
        {
            _lastEnemyPosition = zDoor.Exit.transform.position;
            _lastEnemyLayer = zDoor.Exit.ZLayer;
        }
    }
}
