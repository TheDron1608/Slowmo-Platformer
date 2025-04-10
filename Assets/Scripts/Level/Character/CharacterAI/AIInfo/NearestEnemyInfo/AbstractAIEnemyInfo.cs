using UnityEngine;

public abstract class AbstractAINearestEnemyInfo : AbstractAIInfo
{
    public float MaxEnemyDetectRange = 10f;

    private CharacterTeam _nearestEnemy;
    private float? _nearestEnemyDistance = null;

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
}
