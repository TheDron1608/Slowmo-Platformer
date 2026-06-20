using UnityEngine;

public abstract class AbstractAINearestEnemyInfo : AbstractAIInfo
{
    public float MaxEnemyDetectRange = 10f;

    protected CharacterTeam _nearestEnemy = null;
    protected float? _nearestEnemyDistance = null;
    protected CharacterTeam _lastEnemy = null;
    protected CharacterTeam _lastHeardEnemy = null;
    protected Vector2? _lastEnemyPosition = null;
    protected Vector2? _lastEnemyPositionOnPlatform = null;
    protected ZIndexLayer _lastEnemyLayer = null;
    protected float _timeSinceLastEnemyDetection = 999f;
    protected float _timeSinceLastHeardEnemy = 999f;

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

    public CharacterTeam LastHeardEnemy
    {
        get
        {
            return _lastHeardEnemy;
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
    public Vector2? LastEnemyPositionOnPlatform
    {
        get
        {
            TryUpdateInfo();
            return _lastEnemyPositionOnPlatform;
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

    public float TimeSinceLastHeardEnemy
    {
        get => _timeSinceLastHeardEnemy;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        NoiseManager.Instance.OnNoiseCommited += OnNoiseCommited;
    }

    protected abstract void OnNoiseCommited(object sender, NoiseManager.OnNoiseCommitedEventArgs e);

    private void OnDestroy()
    {
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnNoiseCommited -= OnNoiseCommited;
        }
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        _timeSinceLastEnemyDetection += Time.deltaTime;
        _timeSinceLastHeardEnemy += Time.deltaTime;
    }

    protected Vector2? GetPlatformPositionUnderPoint(ZIndexLayer layer, Vector2 position)
    {
        TileManager.NavigationPlatformInfo platform = layer.TileManager.GetPlatformUnderPointContinuous(TileManager.PositionToTilePosition(position));

        if (platform == null)
        {
            return null;
        }
        else
        {
            return new Vector2(position.x, platform.Position.y + 1f);
        }
    }
}
