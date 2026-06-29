using UnityEngine;

public class NearestEnemyInfoKnowForever : AbstractAINearestEnemyInfo
{
    public TeamManager.Teams PrefferedTeam = TeamManager.Teams.PLAYER;

    protected override void OnUpdateInfo()
    {
        float minDistance = MaxEnemyDetectRange;
        ZIndexLayer currentLayer = CharComponents.CharacterCollision.CurrentZLayer;
        CharacterTeam result = null;
        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(PrefferedTeam).GetTeamMembers())
        {
            if (!character.CharComponents.gameObject.activeSelf)
            {
                continue;
            }

            float charDistance = Vector2.Distance(transform.position, character.transform.position);
            if (
                charDistance < minDistance &&
                !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()
                )
            {
                minDistance = charDistance;
                result = character;
            }
        }

        if (result != null)
        {
            _timeSinceLastEnemyDetection = 0f;
        }
        if (_nearestEnemy != null)
        {
            _lastEnemy = _nearestEnemy;
            _lastEnemyLayer = LayerManager.Instance.GetZLayerOfGameObject(_nearestEnemy.gameObject);
            if (_lastEnemyPosition != _nearestEnemy.transform.position)
            {
                _lastEnemyPositionOnPlatform = GetPlatformPositionUnderPoint(_lastEnemyLayer, _nearestEnemy.transform.position);
            }
            _lastEnemyPosition = _nearestEnemy.transform.position;
        }

        _nearestEnemy = result;
        _nearestEnemyDistance = minDistance;

        if (_nearestEnemy == null && (_lastEnemy?.CharComponents.CharacterInteract.LastInteractObject?.TryGetComponent(out OnInteractEnterMultiZDoor zDoor) ?? false))
        {
            _lastEnemyPosition = zDoor.Exit.transform.position;
            _lastEnemyLayer = zDoor.Exit.ZLayer;
            _lastEnemyPositionOnPlatform = _lastEnemyPosition;
        }
    }

    protected override void OnNoiseCommited(object sender, NoiseManager.OnNoiseCommitedEventArgs e)
    {
    }
}
