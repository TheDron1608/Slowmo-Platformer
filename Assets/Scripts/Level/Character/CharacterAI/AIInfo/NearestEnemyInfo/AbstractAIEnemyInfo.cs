using UnityEngine;

public abstract class AbstractAINearestEnemyInfo : AbstractAIInfo
{
    public float MaxEnemyDetectRange = 10f;

    protected CharacterTeam _nearestEnemy = null;
    protected float? _nearestEnemyDistance = null;
    protected CharacterTeam _lastEnemy = null;
    protected Vector2? _lastEnemyPosition = null;
    protected ZIndexLayer _lastEnemyLayer = null;
    protected float _timeSinceLastEnemyDetection = 999f;

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
}
