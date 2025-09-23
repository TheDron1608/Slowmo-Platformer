using UnityEngine;

public partial class DefaultNearestEnemyInfo : AbstractAINearestEnemyInfo
{
    private CharacterInteractWithObjects _currentInteractTrackCharacter = null;

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
            _lastEnemyLayer = LayerManager.Instance.GetZLayerOfGameObject(_nearestEnemy.gameObject);
        }

        SetCurrentInteractTrackCharacter(_nearestEnemy?.CharComponents.CharacterInteract);
        _nearestEnemy = result;
        _nearestEnemyDistance = minDistance;
    }

    private void SetCurrentInteractTrackCharacter(CharacterInteractWithObjects value)
    {
        if (_currentInteractTrackCharacter != value)
        {
            if (_currentInteractTrackCharacter != null)
            {
                _currentInteractTrackCharacter.OnInteracted -= CurrentInteractTrackCharacter_OnInteracted;
            }
            if (value != null)
            {
                value.OnInteracted += CurrentInteractTrackCharacter_OnInteracted;
            }
            _currentInteractTrackCharacter = value;
        }
    }

    private void CurrentInteractTrackCharacter_OnInteracted(object sender, Interactable e)
    {
        if (e.TryGetComponent(out OnInteractEnterMultiZDoor zDoor))
        {
            _lastEnemyPosition = zDoor.Exit.transform.position;
            _lastEnemyLayer = zDoor.Exit.ZLayer;
        }
    }
}
