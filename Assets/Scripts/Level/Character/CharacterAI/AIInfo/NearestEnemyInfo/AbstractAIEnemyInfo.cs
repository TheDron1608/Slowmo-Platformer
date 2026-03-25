using UnityEngine;

public abstract class AbstractAINearestEnemyInfo : AbstractAIInfo
{
    const float GET_ALLY_ENEMY_INFO_MAX_DISTANCE = 1f;

    public float MaxEnemyDetectRange = 10f;

    protected CharacterTeam _nearestEnemy = null;
    protected float? _nearestEnemyDistance = null;
    protected CharacterTeam _lastEnemy = null;
    protected Vector2? _lastEnemyPosition = null;
    protected ZIndexLayer _lastEnemyLayer = null;
    protected float _timeSinceLastEnemyDetection = 999f;

    private bool _isInheringEnemyInfo = false;

    public CharacterTeam NearestEnemy
    {
        get
        {
            TryUpdateInfo();
            return _nearestEnemy;
        }
    }

    public float? NearestEnemyDistance
    {
        get
        {
            TryUpdateInfo();
            return _nearestEnemyDistance;
        }
    }

    public CharacterTeam LastEnemy
    {
        get
        {
            TryUpdateInfo();
            return _lastEnemy;
        }
    }

    public Vector2? LastEnemyPosition
    {
        get
        {
            TryUpdateInfo();
            return _lastEnemyPosition;
        }
    }
    public ZIndexLayer LastEnemyLayer
    {
        get
        {
            TryUpdateInfo();
            return _lastEnemyLayer;
        }
    }

    public float TimeSinceLastEnemyDetection
    {
        get
        {
            TryUpdateInfo();
            return _timeSinceLastEnemyDetection;
        }
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        _timeSinceLastEnemyDetection += Time.deltaTime;
    }

    protected CharacterTeam TryGetEnemyInfoFromNearAlly()
    {
        /*foreach (Transform characterTransform in LayerManager.Instance.GetZLayerOfGameObject(gameObject).CharactersContainer.transform)
        {
            if (
                characterTransform.gameObject.activeSelf &&
                Vector2.Distance(CharComponents.Center.transform.position, characterTransform.position) < GET_ALLY_ENEMY_INFO_MAX_DISTANCE &&
                characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharComponents.CharacterTeam) &&
                character.CharComponents.CharacterAIManager?.CurrentActiveStateBehaviour?.NearestEnemyInfo != null &&
                !character.CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo._isInheringEnemyInfo &&
                character.CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo._nearestEnemy != null &&
                character.CharComponents != CharComponents
                )
            {
                _isInheringEnemyInfo = true;
                return character.CharComponents.CharacterAIManager?.CurrentActiveStateBehaviour?.NearestEnemyInfo._nearestEnemy;
            }
        }

        _isInheringEnemyInfo = false;*/
        return null;
    }
}
