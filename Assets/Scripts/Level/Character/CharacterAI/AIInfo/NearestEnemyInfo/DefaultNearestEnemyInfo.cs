using UnityEngine;
using UnityEngine.Profiling;

public class DefaultNearestEnemyInfo : AbstractAINearestEnemyInfo
{
    public bool XRay = false;
    public float HearingSensetivityMult = 1f;

    protected virtual bool CharacterCondition(CharacterComponentsManager character)
    {
        return true;
    }

    protected override void OnUpdateInfo()
    {
        Profiler.BeginSample("DefaultNearestEnemyInfo.UpdateInfo");
        float minDistance = MaxEnemyDetectRange;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        CharacterTeam result = null;
        foreach (Transform characterTransform in currentLayer.CharactersContainer.transform)
        {
            if (
                !characterTransform.gameObject.activeSelf ||
                !characterTransform.TryGetComponent(out CharacterComponentsManager character) ||
                !CharacterCondition(character)
                )
            {
                continue;
            }

            float charDistance = Vector2.Distance(transform.position, character.transform.position);
            if (
                charDistance < minDistance &&
                !character.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() &&
                !CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharacterTeam) &&
                (XRay || Physics2D.Linecast(
                    CharComponents.Center.transform.position,
                    character.Center.transform.position,
                    1 << currentLayer.EnviromentLayer
                    ).collider == null
                ))
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
        Profiler.EndSample();
    }

    protected override void OnNoiseCommited(object sender, NoiseManager.OnNoiseCommitedEventArgs e)
    {
        if (NearestEnemy != null) return;

        if (
            e.SourceTeam != null && 
            !CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(e.SourceTeam) &&
            CharComponents.CharacterCollision.CurrentZLayer == e.Layer &&
            Vector2.Distance(CharComponents.Center.transform.position, e.Position) < e.Distance * HearingSensetivityMult
            )
        {
            _timeSinceLastHeardEnemy = 0f;
            _lastHeardEnemy = e.SourceTeam;
            _lastEnemyPosition = e.Position;
            _lastEnemyLayer = e.Layer;
        }
    }
}
