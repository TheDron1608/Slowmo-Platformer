using UnityEngine;

public abstract class AbstractAINearestEnemyInfo : AbstractAIInfo
{
    public float MaxEnemyDetectRange = 10f;

    private CharacterTeam _nearestEnemy;
    private float? _nearestEnemyDistance = null;
    private float _timeSinceLastEnemyDetection = 0f;

    public CharacterTeam NearestEnemy
    {
        get
        {
            TryUpdateInfo();
            return _nearestEnemy;
        }
        protected set => _nearestEnemy = value;
    }

    public float? NearestEnemyDistance
    {
        get
        {
            TryUpdateInfo();
            return _nearestEnemyDistance;
        }
        protected set => _nearestEnemyDistance = value;
    }

    public float TimeSinceLastEnemyDetection
    {
        get
        {
            TryUpdateInfo();
            return _timeSinceLastEnemyDetection;
        }
        protected set => _timeSinceLastEnemyDetection = value;
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        _timeSinceLastEnemyDetection += Time.deltaTime;
    }
}
